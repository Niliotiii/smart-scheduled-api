using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Models;
using SmartScheduledApi.Dtos;
using System.Net;
using SmartScheduledApi.Enums;
using SmartScheduledApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace SmartScheduledApi.Controllers;

[Route("api/teams/{teamId}/[controller]")]
[Authorize]
public class ScheduleController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;
    private readonly AuthorizationService _authService;
    private readonly TeamRulePermissionService _teamRulePermissionService;

    public ScheduleController(
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
    public async Task<IActionResult> GetSchedules(int teamId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        // Verificar permissão do usuário para este time
        if (!_authService.HasAnyTeamRule(userId.Value, teamId, TeamRule.Leader, TeamRule.Editor, TeamRule.Viewer))
        {
            return Forbid();
        }

        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        // Buscar schedules que têm assignments atribuídos a membros deste time
        var schedules = await _context.Scheduleds
            .Where(s => s.Assigneds.Any(a => a.Member.TeamId == teamId))
            .Include(s => s.Assigneds)
                .ThenInclude(a => a.Assignment)
            .Include(s => s.Assigneds)
                .ThenInclude(a => a.Member)  // Incluir Member do Assigned
                .ThenInclude(m => m.User)
            .Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Team = new TeamDto
                {
                    Id = teamId,
                    Name = team.Name
                },
                Assignments = s.Assigneds.Select(a => new ScheduleAssignmentDto
                {
                    Id = a.Assignment.Id,
                    Title = a.Assignment.Title,
                    Description = a.Assignment.Description,
                    AssignedUsers = new List<ScheduleUserDto>
                    {
                        new()
                        {
                            UserId = a.Member.UserId,
                            Name = a.Member.User.Name
                        }
                    }
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse(schedules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule(int teamId, [FromBody] CreateScheduleDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!_authService.HasAnyTeamRule(userId.Value, teamId, TeamRule.Leader, TeamRule.Editor))
        {
            return Forbid();
        }

        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        try
        {
            var schedule = new Scheduled
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Scheduleds.AddAsync(schedule);
            await _context.SaveChangesAsync();

            return Created(schedule);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating schedule", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int teamId, int id, [FromBody] UpdateScheduleDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var schedule = await _context.Scheduleds.FindAsync(id);
        if (schedule == null)
            return NotFound("Schedule not found");

        if (!_authService.HasAnyTeamRule(userId.Value, teamId, TeamRule.Leader, TeamRule.Editor))
        {
            return Forbid();
        }

        try
        {
            schedule.Title = dto.Title;
            schedule.Description = dto.Description;
            schedule.StartDate = dto.StartDate;
            schedule.EndDate = dto.EndDate;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse(schedule, "Schedule updated successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error updating schedule", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int teamId, int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var schedule = await _context.Scheduleds.FindAsync(id);
        if (schedule == null)
            return NotFound("Schedule not found");

        if (!_authService.HasAnyTeamRule(userId.Value, teamId, TeamRule.Leader))
        {
            return Forbid();
        }

        try
        {
            _context.Scheduleds.Remove(schedule);
            await _context.SaveChangesAsync();
            return ApiResponse(schedule, "Schedule deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error deleting schedule", HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
