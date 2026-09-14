using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SEVPMS.Api.Authorization;
using SEVPMS.Api.Bootstrap;
using SEVPMS.Api.Klegar;
using SEVPMS.Api.Middleware;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure;
using SEVPMS.Realtime;
using SEVPMS.Realtime.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

const string FrontendCorsPolicy = "Frontend";
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var allowedOrigins = configuredOrigins.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:4200", "http://localhost:4201", "http://localhost:4202"];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        // An empty production list is valid for the preferred single-origin deployment.
        // Add explicit origins only when the Angular app is hosted on another origin.
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
    options.AddFixedWindowLimiter("sensitive", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer",
        BearerFormat = "JWT", In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwtIssuer,
        ValidateAudience = true, ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrWhiteSpace(accessToken) &&
                (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/events")))
                context.Token = accessToken;
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var rawUserId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(rawUserId, out var userId))
            {
                context.Fail("Authenticated user id is invalid.");
                return;
            }

            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);
            if (user is null || user.Status != AccountStatus.Active)
                context.Fail("Account is not active.");
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CustomerOnly, p => p.RequireRole(UserRole.Customer.ToString()));
    options.AddPolicy(AuthorizationPolicies.EventOrganizerOnly, p => p.RequireRole(UserRole.EventOrganizer.ToString()));
    options.AddPolicy(AuthorizationPolicies.VenueOwnerOnly, p => p.RequireRole(UserRole.VenueOwner.ToString()));
    options.AddPolicy(AuthorizationPolicies.AdminOnly, p => p.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy(AuthorizationPolicies.ParkingManager,
        p => p.RequireRole(UserRole.Admin.ToString(), UserRole.VenueOwner.ToString()));
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRealtime();
builder.Services.AddKlegarBackend();

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();

// When a production Angular build is copied into wwwroot, serve the SPA from the API host.
// Development continues to use the Angular dev server and proxy configuration.
var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var spaIndexPath = Path.Combine(webRoot, "index.html");
var serveSpa = File.Exists(spaIndexPath);
if (app.Environment.IsDevelopment())
{
    await AdminBootstrapSeeder.SeedAsync(app.Services, app.Configuration);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(self), microphone=(), geolocation=(self)";
        headers["X-Frame-Options"] = "DENY";
        headers["Content-Security-Policy"] = "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; img-src 'self' data: blob: https:; media-src 'self' blob: https:; style-src 'self' 'unsafe-inline'; script-src 'self'; connect-src 'self' https: wss: ws:; form-action 'self' https://sandbox.payhere.lk https://www.payhere.lk";
        return Task.CompletedTask;
    });
    await next();
});

// Keep static SPA responses behind the security-header middleware.
if (serveSpa)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<AuditLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<EventHub>("/hubs/events");

if (serveSpa)
{
    // Only browser navigation should fall back to Angular. Unknown API/hub calls must stay real 404s.
    app.MapFallback(async context =>
    {
        var path = context.Request.Path;
        if ((!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            || path.StartsWithSegments("/api")
            || path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(spaIndexPath);
    });
}

app.Run();

public partial class Program;
