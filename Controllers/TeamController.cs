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

[Route("api/[controller]")]
[Authorize]
public class TeamController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;
    private readonly AuthorizationService _authService;
    private readonly TeamRulePermissionService _teamRulePermissionService;

    public TeamController(
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
    public async Task<IActionResult> GetTeams()
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var accessibleTeamIds = await _context.Members
            .Where(m => m.UserId == userId.Value)
            .Select(m => m.TeamId)
            .ToListAsync();

        var teams = await _context.Teams
            .Where(t => accessibleTeamIds.Contains(t.Id))
            .ToListAsync();

        var response = teams
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description
            })
            .ToList();

        return ApiResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        // Apenas administradores podem criar times
        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
            return InvalidRequest("Invalid team data", ModelState);

        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            return Created(team);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating team", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == userId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound("Team not found");

        return ApiResponse(team);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == userId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        if (!_authService.HasAnyTeamRule(userId.Value, id, TeamRule.Leader, TeamRule.Editor))
        {
            return Forbid();
        }

        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound("Team not found");

        try
        {
            team.Name = dto.Name;
            team.Description = dto.Description;
            team.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse(team, "Team updated successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error updating team", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        // Apenas administradores podem deletar times
        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
        {
            return Forbid();
        }

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == userId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        if (!_authService.HasAnyTeamRule(userId.Value, id, TeamRule.Leader))
        {
            return Forbid();
        }

        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound("Team not found");

        try
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return ApiResponse(team, "Team deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error deleting team", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetTeamMembers(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == userId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        var members = await _context.Members
            .Where(m => m.TeamId == id)
            .Include(m => m.User)  // Removed Include for Role
            .Select(m => new TeamMemberResponseDto
            {
                Name = m.User.Name,
                Email = m.User.Email,
                RoleName = m.TeamRule.ToString()  // Using TeamRule directly
            })
            .ToListAsync();

        if (!members.Any())
            return ApiResponse(new List<TeamMemberResponseDto>(), "No members found for this team");

        return ApiResponse(members);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!_authService.HasApplicationRole(userId.Value, ApplicationRole.Administrator))
        {
            return Forbid();
        }

        // var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == userId.Value);

        // if (!isMember)
        //     return InvalidRequest("You do not have access to this team");

        // if (!_authService.HasAnyTeamRule(userId.Value, id, TeamRule.Leader, TeamRule.Editor))
        // {
        //     return Forbid();
        // }

        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound("Team not found");

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound("User not found");

        if (await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == dto.UserId))
            return InvalidRequest("User is already a member of this team");

        try
        {
            var member = new Member
            {
                TeamId = id,
                UserId = dto.UserId,
                TeamRule = dto.TeamRule,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            return Created(member);
        }
        catch (Exception ex)
        {
            return ApiError("Error adding team member", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveTeamMember(int id, int userId)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized();

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == currentUserId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        if (!_authService.HasAnyTeamRule(currentUserId.Value, id, TeamRule.Leader))
        {
            return Forbid();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);

        if (member == null)
            return NotFound("Team member not found");

        try
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return ApiResponse(member, "Member removed successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error removing team member", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}/members/{userId}/role")]
    public async Task<IActionResult> UpdateMemberRole(int id, int userId, [FromBody] UpdateMemberRoleDto dto)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized();

        var isMember = await _context.Members.AnyAsync(m => m.TeamId == id && m.UserId == currentUserId.Value);

        if (!isMember)
            return InvalidRequest("You do not have access to this team");

        if (!_authService.HasAnyTeamRule(currentUserId.Value, id, TeamRule.Leader))
        {
            return Forbid();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);

        if (member == null)
            return NotFound("Team member not found");

        try
        {
            member.TeamRule = dto.TeamRule; 
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ApiResponse(member, "Member role updated successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error updating member role", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("my-teams")]
    public async Task<IActionResult> GetMyTeams()
    {
        var teamRules = await _userContext.GetUserTeamRulesAsync();
        if (!teamRules.Any())
            return ApiResponse(new List<UserTeamRule>(), "No team memberships found");

        return ApiResponse(teamRules);
    }

    [HttpGet("{id}/my-role")]
    public async Task<IActionResult> GetMyRoleInTeam(int id)
    {
        var membership = await _userContext.GetCurrentMembershipAsync(id);
        if (membership == null)
            return NotFound("You are not a member of this team");

        return ApiResponse(new
        {
            TeamId = id,
            Role = membership.TeamRule.ToString(),  // Using TeamRule directly
            Permissions = _teamRulePermissionService.GetPermissions(membership.TeamRule)
        });
    }
}
