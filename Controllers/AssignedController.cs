using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Controllers;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Models;
using SmartScheduledApi.Dtos;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SmartScheduledApi.Services;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AssignedController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;
    private readonly AuthorizationService _authService;
    private readonly TeamRulePermissionService _teamRulePermissionService;

    public AssignedController(
        SmartScheduleApiContext context,
        UserContextService userContext,
        AuthorizationService authService,
        TeamRulePermissionService teamRulePermissionService)
    {
        _context = context;
        _userContext = userContext;
        _authService = authService;
        _teamRulePermissionService = teamRulePermissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssigned()
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

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

        // Verificar se o membro pertence ao time e se o usuário tem permissão
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

        if (member == null)
            return NotFound("Member not found");

        if (!_authService.HasAnyTeamRule(userId.Value, member.TeamId, TeamRule.Leader, TeamRule.Editor))
        {
            return Forbid();
        }

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
    public async Task<IActionResult> GetAssigned(int id)
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

        if (!_authService.HasAnyTeamRule(userId.Value, assigned.Member.TeamId, TeamRule.Leader))
        {
            return Forbid();
        }

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

    [HttpGet("schedule/{scheduleId}")]
    public async Task<IActionResult> GetBySchedule(int scheduleId)
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

        if (!_authService.HasAnyTeamRule(userId.Value, firstAssigned.Member.TeamId,
            TeamRule.Leader, TeamRule.Editor, TeamRule.Viewer))
        {
            return Forbid();
        }

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

        // Pode ver apenas seus próprios assignments ou ser administrador
        if (currentUserId != userId && !_authService.HasApplicationRole(currentUserId.Value, ApplicationRole.Administrator))
        {
            return Forbid();
        }

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
}
