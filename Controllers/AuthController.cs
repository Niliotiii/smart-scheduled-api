using Microsoft.AspNetCore.Mvc;
using SmartScheduledApi.Dtos;
using SmartScheduledApi.Services;
using System.Net;
using SmartScheduledApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartScheduledApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserContextService _userContext;


    public AuthController(IAuthService authService, UserContextService userContext)
    {
        _authService = authService;
        _userContext = userContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.Login(dto);
        if (token == null)
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(new { access_token = token });
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _authService.Register(dto);
        if (!result)
        {
            return BadRequest("Registration failed");
        }

        return Ok("Registration successful");
    }

    [HttpPost("reset-password")]
    [Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (dto == null)
            return BadRequest("Invalid reset password data");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine($"userId: {userId}");
        Console.WriteLine($"dto.UserId: {dto.UserId}");
        Console.WriteLine($"userId == null || userId != dto.UserId.ToString(): {userId == null || userId != dto.UserId.ToString()}");

        if (userId == null || userId != dto.UserId.ToString())
            return Unauthorized("You are not authorized to reset this password");

        var result = await _authService.ResetPassword(dto);
        if (!result)
            return StatusCode((int)HttpStatusCode.InternalServerError, "Error resetting password");

        return Ok("Password reset successfully");
    }
}
