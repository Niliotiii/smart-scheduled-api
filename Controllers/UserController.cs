using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Models;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Dtos;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SmartScheduledApi.Services;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UserController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly UserContextService _userContext;
    private readonly AuthorizationService _authService;
    private readonly TeamRulePermissionService _teamRulePermissionService;

    public UserController(
        SmartScheduleApiContext context,
        IPasswordHasher<User> passwordHasher,
        UserContextService userContext,
        AuthorizationService authService,
        TeamRulePermissionService teamRulePermissionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _userContext = userContext;
        _authService = authService;
        _teamRulePermissionService = teamRulePermissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Cpf = u.Cpf,
                Cellphone = u.Cellphone,
                MotherName = u.MotherName,
                FatherName = u.FatherName,
                MotherCellphone = u.MotherCellphone,
                FatherCellphone = u.FatherCellphone,
                Street = u.Address.Street,
                City = u.Address.City,
                State = u.Address.State,
                PostalCode = u.Address.PostalCode,
                Country = u.Address.Country
            })
            .ToListAsync();

        return ApiResponse(users);
    }

    // Já está correto: apenas administradores podem criar usuários
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
            return Forbid();

        if (!ModelState.IsValid)
            return InvalidRequest("Invalid user data", ModelState);

        try
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

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Cpf = user.Cpf,
                Cellphone = user.Cellphone,
                MotherName = user.MotherName,
                FatherName = user.FatherName,
                MotherCellphone = user.MotherCellphone,
                FatherCellphone = user.FatherCellphone,
                Street = user.Address.Street,
                City = user.Address.City,
                State = user.Address.State,
                PostalCode = user.Address.PostalCode,
                Country = user.Address.Country
            };

            return Created(response);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating user", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Cpf = u.Cpf,
                Cellphone = u.Cellphone,
                MotherName = u.MotherName,
                FatherName = u.FatherName,
                MotherCellphone = u.MotherCellphone,
                FatherCellphone = u.FatherCellphone,
                Street = u.Address.Street,
                City = u.Address.City,
                State = u.Address.State,
                PostalCode = u.Address.PostalCode,
                Country = u.Address.Country
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("User not found");

        return ApiResponse(user);
    }

    [HttpGet("{id}/teams")]
    public async Task<IActionResult> GetUserTeams(int id)
    {
        var teams = await _context.Members
            .Where(m => m.UserId == id)
            .Include(m => m.Team)
            .Select(m => new UserTeamDto
            {
                Id = m.Team.Id,
                Name = m.Team.Name,
                Description = m.Team.Description,
                TeamRule = m.TeamRule.ToString()
            })
            .ToListAsync();

        if (!teams.Any())
            return ApiResponse(new List<UserTeamDto>(), "No teams found for this user");

        return ApiResponse(teams);
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int id)
    {
        var permissions = await _context.Members
            .Where(m => m.UserId == id)
            .Include(m => m.Team)
            .Select(m => new UserPermissionDto
            {
                PermissionTeamId = m.TeamId,
                PermissionTeamName = m.Team.Name,
                PermissionTeamRule = m.TeamRule.ToString()
            })
            .ToListAsync();

        if (!permissions.Any())
            return ApiResponse(new List<UserPermissionDto>(), "No permissions found for this user");

        return ApiResponse(permissions);
    }

    [HttpGet("{id}/schedules")]
    public async Task<IActionResult> GetUserSchedules(int id)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized();

        // Pode ver apenas seus próprios schedules ou ser administrador
        if (currentUserId != id && !_authService.HasApplicationRole(currentUserId.Value, ApplicationRole.Administrator))
        {
            return Forbid();
        }

        // Atualizar para usar a relação correta entre Member e Assigned
        var schedules = await _context.Assigneds
            .Where(a => a.Member.UserId == id)
            .Include(a => a.Scheduled)
            .Include(a => a.Assignment)
            .Select(a => new UserScheduleDto
            {
                Id = a.Scheduled.Id,
                Title = a.Scheduled.Title,
                Description = a.Scheduled.Description,
                StartDate = a.Scheduled.StartDate,
                EndDate = a.Scheduled.EndDate,
                Assignment = new UserAssignmentDto
                {
                    Id = a.Assignment.Id,
                    Title = a.Assignment.Title,
                    Description = a.Assignment.Description
                }
            })
            .ToListAsync();

        if (!schedules.Any())
            return ApiResponse(new List<UserScheduleDto>(), "No schedules found for this user");

        return ApiResponse(schedules);
    }

    // Já está correto: apenas administradores podem atualizar usuários
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
            return Forbid();

        var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound("User not found");

        try
        {
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Cpf = dto.Cpf;
            user.Cellphone = dto.Cellphone;
            user.MotherName = dto.MotherName;
            user.FatherName = dto.FatherName;
            user.MotherCellphone = dto.MotherCellphone;
            user.FatherCellphone = dto.FatherCellphone;

            if (user.Address == null)
            {
                user.Address = new Address();
            }

            user.Address.Street = dto.Street;
            user.Address.City = dto.City;
            user.Address.State = dto.State;
            user.Address.PostalCode = dto.PostalCode;
            user.Address.Country = dto.Country;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Cpf = user.Cpf,
                Cellphone = user.Cellphone,
                MotherName = user.MotherName,
                FatherName = user.FatherName,
                MotherCellphone = user.MotherCellphone,
                FatherCellphone = user.FatherCellphone,
                Street = user.Address.Street,
                City = user.Address.City,
                State = user.Address.State,
                PostalCode = user.Address.PostalCode,
                Country = user.Address.Country
            };

            return ApiResponse(response, "User updated successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error updating user", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    // Já está correto: apenas administradores podem excluir usuários
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
            return Forbid();

        var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound("User not found");

        try
        {
            user.DeletedAt = DateTime.UtcNow;

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Cpf = user.Cpf,
                Cellphone = user.Cellphone,
                MotherName = user.MotherName,
                FatherName = user.FatherName,
                MotherCellphone = user.MotherCellphone,
                FatherCellphone = user.FatherCellphone,
                Street = user.Address.Street,
                City = user.Address.City,
                State = user.Address.State,
                PostalCode = user.Address.PostalCode,
                Country = user.Address.Country
            };

            await _context.SaveChangesAsync();
            return ApiResponse(response, "User deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error deleting user", HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
