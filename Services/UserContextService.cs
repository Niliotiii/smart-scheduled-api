using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Models;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Dtos;

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

    public async Task<IEnumerable<UserTeamRule>> GetUserTeamRulesAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Enumerable.Empty<UserTeamRule>();

        return await _context.Members
            .Where(m => m.UserId == userId)
            .Include(m => m.Team)
            .Select(m => new UserTeamRule
            {
                TeamId = m.TeamId,
                TeamName = m.Team.Name,
                TeamRuleId = (int)m.TeamRule,
                TeamRuleName = m.TeamRule.ToString()
            })
            .ToListAsync();
    }

    public int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}
