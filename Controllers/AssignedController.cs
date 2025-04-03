using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Models;
using SmartScheduledApi.Dtos;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SmartScheduledApi.Services;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Controllers;

[Route("api/teams/{teamId}/[controller]")]  // Update route to include teamId
[Authorize]
public class AssignedController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;

    public AssignedController(
        SmartScheduleApiContext context,
        UserContextService userContext,
        IPermissionService permissionService) : base(permissionService)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssigned(int teamId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewSchedules))
            return Forbidden("You don't have permission to view assignments");

        var assigneds = await _context.Assigneds
            .Include(a => a.Scheduled)
            .Include(a => a.Assignment)
            .Include(a => a.Member)  // Usar Member direto do Assigned
                .ThenInclude(m => m.User)
            .Select(a => new AssignedResponseDto
            {
                Id = a.Id,
                Schedule = new AssignedScheduleDto
                {
                    Id = a.Scheduled.Id,
                    Title = a.Scheduled.Title,
                    StartDate = a.Scheduled.StartDate,
                    EndDate = a.Scheduled.EndDate
                },
                Assignment = new AssignedAssignmentDto
                {
                    Id = a.Assignment.Id,
                    Title = a.Assignment.Title,
                    Description = a.Assignment.Description,
                    User = new UserDto
                    {
                        Id = a.Member.User.Id,  // Usar Member direto do Assigned
                        Name = a.Member.User.Name,  // Usar Member direto do Assigned
                        Email = a.Member.User.Email  // Usar Member direto do Assigned
                    }
                },
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(assigneds);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssigned([FromBody] CreateAssignedDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

        if (member == null)
            return NotFound("Member not found");

        if (!await EnsureTeamPermissionAsync(userId.Value, member.TeamId, TeamPermission.ManageTeamSettings))
            return Forbidden("You don't have permission to manage assignments");

        if (!ModelState.IsValid)
            return InvalidRequest("Invalid assigned data", ModelState);

        var scheduled = await _context.Scheduleds.FindAsync(dto.ScheduledId);
        if (scheduled == null)
            return NotFound("Schedule not found");

        var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
        if (assignment == null)
            return NotFound("Assignment not found");

        try
        {
            var assigned = new Assigned
            {
                ScheduledId = dto.ScheduledId,
                AssignmentId = dto.AssignmentId,
                MemberId = dto.MemberId,  // Novo campo
                CreatedAt = DateTime.UtcNow
            };

            await _context.Assigneds.AddAsync(assigned);
            await _context.SaveChangesAsync();

            return Created(assigned);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating assigned", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssignedById(int id)
    {
        var assigned = await _context.Assigneds
            .Include(a => a.Scheduled)
            .Include(a => a.Assignment)
            .Include(a => a.Member)  // Usar Member direto do Assigned
                .ThenInclude(m => m.User)
            .Select(a => new AssignedResponseDto
            {
                Id = a.Id,
                Schedule = new AssignedScheduleDto
                {
                    Id = a.Scheduled.Id,
                    Title = a.Scheduled.Title,
                    StartDate = a.Scheduled.StartDate,
                    EndDate = a.Scheduled.EndDate
                },
                Assignment = new AssignedAssignmentDto
                {
                    Id = a.Assignment.Id,
                    Title = a.Assignment.Title,
                    Description = a.Assignment.Description,
                    User = new UserDto
                    {
                        Id = a.Member.User.Id,  // Usar Member direto do Assigned
                        Name = a.Member.User.Name,  // Usar Member direto do Assigned
                        Email = a.Member.User.Email  // Usar Member direto do Assigned
                    }
                },
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assigned == null)
            return NotFound("Assigned not found");

        return ApiResponse(assigned);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssigned(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var assigned = await _context.Assigneds
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assigned == null)
            return NotFound("Assigned not found");

        if (!await EnsureTeamPermissionAsync(userId.Value, assigned.Member.TeamId, TeamPermission.ManageTeamSettings))
            return Forbidden("You don't have permission to delete assignments");

        try
        {
            _context.Assigneds.Remove(assigned);
            await _context.SaveChangesAsync();
            return ApiResponse(assigned, "Assigned deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error deleting assigned", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    // Remove the teamId from this route since it's already in the controller route
    [HttpGet("schedule/{scheduleId}")]
    public async Task<IActionResult> GetBySchedule(int teamId, int scheduleId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var schedule = await _context.Scheduleds
            .Include(s => s.Assigneds)
                .ThenInclude(a => a.Member)  // Usar Member direto do Assigned
            .FirstOrDefaultAsync(s => s.Id == scheduleId);

        if (schedule == null)
            return NotFound("Schedule not found");

        // Verificar se o usuário tem acesso a este time
        var firstAssigned = schedule.Assigneds.FirstOrDefault();
        if (firstAssigned == null)
            return NotFound("No assignments found for this schedule");

        if (!await EnsureTeamPermissionAsync(userId.Value, firstAssigned.Member.TeamId, TeamPermission.ViewSchedules))
            return Forbidden("You don't have permission to view assignments");

        var assigneds = await _context.Assigneds
            .Where(a => a.ScheduledId == scheduleId)
            .Include(a => a.Assignment)
            .Include(a => a.Member)  // Usar Member direto do Assigned
                .ThenInclude(m => m.User)
            .Select(a => new AssignedAssignmentDto
            {
                Id = a.Assignment.Id,
                Title = a.Assignment.Title,
                Description = a.Assignment.Description,
                User = new UserDto
                {
                    Id = a.Member.User.Id,  // Usar Member direto do Assigned
                    Name = a.Member.User.Name,  // Usar Member direto do Assigned
                    Email = a.Member.User.Email  // Usar Member direto do Assigned
                }
            })
            .ToListAsync();

        return ApiResponse(assigneds);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized();

        if (currentUserId != userId && !await EnsureApplicationPermissionAsync(currentUserId.Value, ApplicationPermission.ManageSystem))
            return Forbidden("You don't have permission to view other users' assignments");

        var assigneds = await _context.Assigneds
            .Where(a => a.Member.UserId == userId)  // Usar Member direto do Assigned
            .Include(a => a.Scheduled)
            .Select(a => new AssignedScheduleDto
            {
                Id = a.Scheduled.Id,
                Title = a.Scheduled.Title,
                StartDate = a.Scheduled.StartDate,
                EndDate = a.Scheduled.EndDate
            })
            .ToListAsync();

        return ApiResponse(assigneds);
    }

    // Remove "team/{teamId}" since teamId is already in controller route
    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedByTeam(int teamId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewSchedules))
            return Forbidden("You don't have permission to view assignments");

        var assigneds = await _context.Assigneds
            .Include(a => a.Scheduled)
            .Include(a => a.Assignment)
            .Include(a => a.Member)  // Usar Member direto do Assigned
                .ThenInclude(m => m.User)
            .Select(a => new AssignedResponseDto
            {
                Id = a.Id,
                Schedule = new AssignedScheduleDto
                {
                    Id = a.Scheduled.Id,
                    Title = a.Scheduled.Title,
                    StartDate = a.Scheduled.StartDate,
                    EndDate = a.Scheduled.EndDate
                },
                Assignment = new AssignedAssignmentDto
                {
                    Id = a.Assignment.Id,
                    Title = a.Assignment.Title,
                    Description = a.Assignment.Description,
                    User = new UserDto
                    {
                        Id = a.Member.User.Id,  // Usar Member direto do Assigned
                        Name = a.Member.User.Name,  // Usar Member direto do Assigned
                        Email = a.Member.User.Email  // Usar Member direto do Assigned
                    }
                },
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(assigneds);
    }
}
