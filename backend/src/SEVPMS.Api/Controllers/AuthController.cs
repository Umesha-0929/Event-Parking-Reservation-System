using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IAccountSecurityService accountSecurityService,
    IWebHostEnvironment environment)
    : ControllerBase
{
    private const string RefreshCookieName = "nvent_refresh";

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegistrationPendingResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created,
            await authService.RegisterAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("verify-email-otp")]
    public async Task<ActionResult<EmailVerificationResponse>> VerifyEmailOtp(
        [FromBody] VerifyEmailOtpRequest request,
        CancellationToken cancellationToken)
        => Ok(await authService.VerifyEmailOtpAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("resend-email-otp")]
    public async Task<ActionResult<RegistrationPendingResponse>> ResendEmailOtp(
        [FromBody] ResendEmailOtpRequest request,
        CancellationToken cancellationToken)
        => Ok(await authService.ResendEmailOtpAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);
        response.RefreshToken = string.Empty;
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName]
                           ?? request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { error = "refresh_token_required" });

        var response = await authService.RefreshTokenAsync(
            new RefreshTokenRequest { RefreshToken = refreshToken },
            cancellationToken);

        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);
        response.RefreshToken = string.Empty;
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var userId)) return Unauthorized();

        request ??= new LogoutRequest();
        request.RefreshToken ??= Request.Cookies[RefreshCookieName];
        await accountSecurityService.LogoutAsync(userId, request, cancellationToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await accountSecurityService.RequestPasswordResetAsync(request, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await accountSecurityService.ConfirmPasswordResetAsync(request, cancellationToken);
        return NoContent();
    }

    private void SetRefreshCookie(string token, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(
            RefreshCookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || !environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                Expires = new DateTimeOffset(DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc)),
                IsEssential = true
            });
    }

    private void DeleteRefreshCookie()
        => Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || !environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth"
            });
}
