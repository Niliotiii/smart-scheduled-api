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

[Route("api/[controller]")]
[Authorize]
public class InviteController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;
    public InviteController(
        SmartScheduleApiContext context,
        UserContextService userContext,
        IPermissionService permissionService) : base(permissionService)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyInvites()
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var invites = await _context.Invites
            .Where(i => i.UserId == userId.Value && i.Status == InviteStatus.Waiting)
            .Include(i => i.Team)
            .Include(i => i.InvitedBy)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                UserId = i.UserId,
                InvitedById = i.InvitedById,
                InvitedByName = i.InvitedBy.Name,
                TeamRule = i.TeamRule,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(invites);
    }

    [HttpGet("user/{userId}/pending")]
    public async Task<IActionResult> GetUserPendingInvites(int userId)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized();

        if (userId != currentUserId && !await EnsureApplicationPermissionAsync(currentUserId.Value, ApplicationPermission.ViewUsers))
            return Forbidden("You don't have permission to view other users' invites");

        var invites = await _context.Invites
            .Where(i => i.UserId == userId && i.Status == InviteStatus.Waiting)
            .Include(i => i.Team)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                UserId = i.UserId,
                InvitedById = i.InvitedById,
                InvitedByName = i.InvitedBy.Name,
                TeamRule = i.TeamRule,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(invites);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvite(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var invite = await _context.Invites
            .Where(i => i.Id == id)
            .Include(i => i.Team)
            .Include(i => i.User)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync();

        if (invite == null)
            return NotFound("Invite not found");

        if (invite.UserId != userId &&
            invite.InvitedById != userId &&
            !await EnsureApplicationPermissionAsync(userId.Value, ApplicationPermission.ManageSystem))
        {
            return Forbid();
        }

        var response = new InviteDto
        {
            Id = invite.Id,
            TeamId = invite.TeamId,
            TeamName = invite.Team.Name,
            UserId = invite.UserId,
            UserName = invite.User.Name,
            InvitedById = invite.InvitedById,
            InvitedByName = invite.InvitedBy.Name,
            TeamRule = invite.TeamRule,
            Status = invite.Status,
            CreatedAt = invite.CreatedAt,
            UpdatedAt = invite.UpdatedAt
        };

        return ApiResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvite([FromBody] CreateInviteDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, dto.TeamId, TeamPermission.ManageInvites))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
            return InvalidRequest("Invalid invite data", ModelState);

        var team = await _context.Teams.FindAsync(dto.TeamId);
        if (team == null)
            return NotFound("Team not found");

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound("User not found");

        if (await _context.Invites.AnyAsync(
            i => i.TeamId == dto.TeamId &&
                 i.UserId == dto.UserId &&
                 i.Status == InviteStatus.Waiting))
        {
            return InvalidRequest("User already has a pending invite to this team");
        }

        if (await _context.Members.AnyAsync(
            m => m.TeamId == dto.TeamId &&
                 m.UserId == dto.UserId))
        {
            return InvalidRequest("User is already a member of this team");
        }

        try
        {
            var invite = new Invite
            {
                TeamId = dto.TeamId,
                UserId = dto.UserId,
                InvitedById = userId.Value,
                TeamRule = dto.TeamRule,
                Status = InviteStatus.Waiting,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Invites.AddAsync(invite);
            await _context.SaveChangesAsync();

            var response = new InviteDto
            {
                Id = invite.Id,
                TeamId = team.Id,
                TeamName = team.Name,
                UserId = user.Id,
                UserName = user.Name,
                InvitedById = userId.Value,
                InvitedByName = (await _context.Users.FindAsync(userId.Value))?.Name,
                TeamRule = invite.TeamRule,
                Status = invite.Status,
                CreatedAt = invite.CreatedAt
            };

            return Created(response);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating invite", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}/accept")]
    public async Task<IActionResult> AcceptInvite(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var invite = await _context.Invites.FindAsync(id);
        if (invite == null)
            return NotFound("Invite not found");

        if (invite.UserId != userId)
            return Forbid();

        if (invite.Status != InviteStatus.Waiting)
            return InvalidRequest($"Invite is already {invite.Status}");

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                invite.Status = InviteStatus.Accepted;
                invite.UpdatedAt = DateTime.UtcNow;

                var member = new Member
                {
                    UserId = invite.UserId,
                    TeamId = invite.TeamId,
                    TeamRule = invite.TeamRule,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Members.AddAsync(member);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse(invite, "Invite accepted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiError("Error accepting invite", HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectInvite(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var invite = await _context.Invites.FindAsync(id);
        if (invite == null)
            return NotFound("Invite not found");

        if (invite.UserId != userId)
            return Forbid();

        if (invite.Status != InviteStatus.Waiting)
            return InvalidRequest($"Invite is already {invite.Status}");

        try
        {
            invite.Status = InviteStatus.Rejected;
            invite.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse(invite, "Invite rejected successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error rejecting invite", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelInvite(int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var invite = await _context.Invites.FindAsync(id);
        if (invite == null)
            return NotFound("Invite not found");

        if (invite.InvitedById != userId && !await EnsureApplicationPermissionAsync(userId.Value, ApplicationPermission.ManageSystem))
            return Forbidden("You don't have permission to cancel this invite");

        if (invite.Status != InviteStatus.Waiting)
            return InvalidRequest($"Cannot cancel invite that is {invite.Status}");

        try
        {
            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();
            return ApiResponse(invite, "Invite cancelled successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error cancelling invite", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetTeamInvites(int teamId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewTeam))
            return Forbidden("You don't have permission to view team invites");

        var invites = await _context.Invites
            .Where(i => i.TeamId == teamId)
            .Include(i => i.User)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                TeamId = i.TeamId,
                UserId = i.UserId,
                UserName = i.User.Name,
                InvitedById = i.InvitedById,
                InvitedByName = i.InvitedBy.Name,
                TeamRule = i.TeamRule,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(invites);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingInvites()
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureApplicationPermissionAsync(userId.Value, ApplicationPermission.ManageSystem))
            return Forbidden("You don't have permission to view pending invites");

        var invites = await _context.Invites
            .Where(i => i.Status == InviteStatus.Waiting)
            .Include(i => i.Team)
            .Include(i => i.User)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                UserId = i.UserId,
                UserName = i.User.Name,
                InvitedById = i.InvitedById,
                InvitedByName = i.InvitedBy.Name,
                TeamRule = i.TeamRule,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse(invites);
    }
}