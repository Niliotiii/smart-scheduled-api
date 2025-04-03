using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Models;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Services;

public class UserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SmartScheduleApiContext _context;

    public UserContextService(IHttpContextAccessor httpContextAccessor, SmartScheduleApiContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<Member?> GetCurrentMembershipAsync(int teamId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return null;

        return await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamId == teamId);
    }

    public async Task<IEnumerable<UserTeamRole>> GetUserTeamRolesAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Enumerable.Empty<UserTeamRole>();

        return await _context.Members
            .Where(m => m.UserId == userId)
            .Include(m => m.Team)
            .Select(m => new UserTeamRole
            {
                TeamId = m.TeamId,
                TeamName = m.Team.Name,
                TeamRuleId = (int)m.TeamRule,  // Changed from RoleId
                TeamRuleName = m.TeamRule.ToString()  // Changed from RoleName
            })
            .ToListAsync();
    }

    public async Task<string> GetCurrentRoleAsync(int teamId)
    {
        var membership = await GetCurrentMembershipAsync(teamId);
        return membership?.TeamRule.ToString() ?? string.Empty;
    }

    public int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}

public class UserTeamRole
{
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public int TeamRuleId { get; set; }  // Changed from RoleId
    public string TeamRuleName { get; set; }  // Changed from RoleName
}
