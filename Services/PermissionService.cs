using SmartScheduledApi.Enums;
using SmartScheduledApi.DataContext;
using Microsoft.EntityFrameworkCore;

namespace SmartScheduledApi.Services
{
    public interface IPermissionService
    {
        Task<bool> HasApplicationPermissionAsync(int userId, ApplicationPermission permission);
        Task<bool> HasTeamPermissionAsync(int userId, int teamId, TeamPermission permission);
        Task<ApplicationPermission> GetApplicationPermissionsAsync(int userId);
        Task<TeamPermission> GetTeamPermissionsAsync(int userId, int teamId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly SmartScheduleApiContext _context;

        public PermissionService(SmartScheduleApiContext context)
        {
            _context = context;
        }

        public async Task<bool> HasApplicationPermissionAsync(int userId, ApplicationPermission permission)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var userPermissions = PermissionMapping.ApplicationPermissions[user.ApplicationRole];
            return userPermissions.HasFlag(permission);
        }

        public async Task<bool> HasTeamPermissionAsync(int userId, int teamId, TeamPermission permission)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamId == teamId);

            if (member == null) return false;

            var teamPermissions = PermissionMapping.TeamPermissions[member.TeamRule];
            return teamPermissions.HasFlag(permission);
        }

        public async Task<ApplicationPermission> GetApplicationPermissionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApplicationPermission.None;

            return PermissionMapping.ApplicationPermissions[user.ApplicationRole];
        }

        public async Task<TeamPermission> GetTeamPermissionsAsync(int userId, int teamId)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamId == teamId);

            if (member == null) return TeamPermission.None;

            return PermissionMapping.TeamPermissions[member.TeamRule];
        }
    }
}
