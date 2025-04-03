using SmartScheduledApi.Enums;
using SmartScheduledApi.DataContext;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Models;

namespace SmartScheduledApi.Services;

public class AuthorizationService
{
    private readonly SmartScheduleApiContext _context;

    public AuthorizationService(SmartScheduleApiContext context)
    {
        _context = context;
    }

    public bool HasApplicationRole(int userId, ApplicationRole role)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        return user?.ApplicationRole == role;
    }

    public bool HasAnyTeamRule(int userId, int teamId, params TeamRule[] rules)
    {
        var membership = _context.Members
            .FirstOrDefault(m => m.UserId == userId && m.TeamId == teamId);

        if (membership == null) return false;

        return rules.Contains(membership.TeamRule);
    }
}