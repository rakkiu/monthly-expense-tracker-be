using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Features.Auth.Commands;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RegisterCommand(request));
            return Ok(new { success = true, data = result, message = "Registration successful" });
        }
        catch (DomainException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _mediator.Send(new LoginCommand(request));
            SetRefreshTokenCookie(result.AccessToken);
            return Ok(new { success = true, data = result, message = "Login successful" });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { success = false, error = "No refresh token" });

        try
        {
            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            SetRefreshTokenCookie(result.AccessToken);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
            await _mediator.Send(new LogoutCommand(refreshToken));

        Response.Cookies.Delete("refreshToken");
        return Ok(new { success = true, message = "Logged out" });
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _mediator.Send(new ChangePasswordCommand(
            request.CurrentPassword, request.NewPassword, request.ConfirmNewPassword));
        return Ok(new { success = true, message = "Password changed" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
