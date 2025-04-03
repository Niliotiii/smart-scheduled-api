using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using SmartScheduledApi.Dtos;
using SmartScheduledApi.Models;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Interfaces;

namespace SmartScheduledApi.Services;

public class AuthService : IAuthService
{
    private readonly SmartScheduleApiContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(SmartScheduleApiContext context, IPasswordHasher<User> passwordHasher, ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string?> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !IsBase64String(user.Password) || _passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password) == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    public async Task<bool> Register(RegisterDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Name = dto.Name,
            Cpf = dto.Cpf,
            Cellphone = dto.Cellphone,
            MotherName = dto.MotherName,
            FatherName = dto.FatherName,
            MotherCellphone = dto.MotherCellphone,
            FatherCellphone = dto.FatherCellphone,
            Address = new Address
            {
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country
            }
        };

        user.Password = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null)
            return false;

        if (_passwordHasher.VerifyHashedPassword(user, user.Password, dto.OldPassword) == PasswordVerificationResult.Failed)
            return false;

        try
        {
            user.Password = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? GenerateJwtToken(User user)
    {
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");


        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            _logger.LogError("JWT configuration is not set");
            return null;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, user.Username),
            new Claim(ClaimTypes.Role, user.ApplicationRole.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}
