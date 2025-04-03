using SmartScheduledApi.Dtos;

namespace SmartScheduled.Api.Services;

public interface IAuthService
{
    Task<bool> Login(LoginDto dto);
    Task<bool> Register(RegisterDto dto);
    Task<bool> ResetPassword(ResetPasswordDto dto);
    Task<bool> ValidateUserAsync(string username, string password);
}
