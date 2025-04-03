using SmartScheduledApi.Dtos;

namespace SmartScheduledApi.Interfaces;

public interface IAuthService
{
    Task<string?> Login(LoginDto dto);
    Task<bool> Register(RegisterDto dto);
    Task<bool> ResetPassword(ResetPasswordDto dto);
}
