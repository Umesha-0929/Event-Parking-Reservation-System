using SEVPMS.Api.Klegar;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SEVPMS.Api.Middleware;
using SEVPMS.Infrastructure;
using SEVPMS.Realtime;
using SEVPMS.Realtime.Hubs;
using Microsoft.OpenApi.Models;
using SEVPMS.Api.Authorization;
using SEVPMS.Domain.Enums;
using SEVPMS.Api.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

const string AngularDevCorsPolicy = "AngularDev";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AngularDevCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:4201",
                    "http://localhost:4202")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter your JWT access token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// =============================================
// JWT Authentication
// =============================================

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30)
            };

        options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken =
                    context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        (path.StartsWithSegments("/hubs/notifications") ||
                        path.StartsWithSegments("/hubs/events")))
                        {
                            context.Token = accessToken;
                        }

                    return Task.CompletedTask;
                }
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.CustomerOnly,
        policy =>
            policy.RequireRole(
                UserRole.Customer.ToString()));

    options.AddPolicy(
        AuthorizationPolicies.EventOrganizerOnly,
        policy =>
            policy.RequireRole(
                UserRole.EventOrganizer.ToString()));

    options.AddPolicy(
        AuthorizationPolicies.VenueOwnerOnly,
        policy =>
            policy.RequireRole(
                UserRole.VenueOwner.ToString()));

    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy =>
            policy.RequireRole(
                UserRole.Admin.ToString()));
});

// =============================================
// Project Services
// =============================================

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddRealtime();
builder.Services.AddKlegarBackend();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await AdminBootstrapSeeder.SeedAsync(
        app.Services,
        app.Configuration);
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =============================================
// Middleware
// =============================================

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

// IMPORTANT: Authentication first
app.UseAuthentication();
app.UseAuthorization();

// =============================================
// Endpoints
// =============================================

app.MapControllers();

app.MapHub<NotificationHub>(
    "/hubs/notifications");

app.MapHub<EventHub>(
    "/hubs/events");

app.Run();

public partial class Program;
