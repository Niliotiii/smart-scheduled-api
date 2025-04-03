using System.Text.Json;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Services
{
    public class TeamRulePermissionService
    {
        private static readonly Dictionary<TeamRule, Permission> _permissionsMap = new Dictionary<TeamRule, Permission>
        {
            [TeamRule.Leader] = Permission.ViewTeam
                | Permission.EditTeam
                | Permission.DeleteTeam
                | Permission.ManageMembers
                | Permission.CreateSchedule
                | Permission.EditSchedule
                | Permission.DeleteSchedule
                | Permission.AssignMembers
                | Permission.ViewSchedule,
            [TeamRule.Editor] = Permission.ViewTeam
                | Permission.EditTeam
                | Permission.CreateSchedule
                | Permission.EditSchedule
                | Permission.AssignMembers
                | Permission.ViewSchedule,
            [TeamRule.Viewer] = Permission.ViewTeam
                | Permission.ViewSchedule
        };

        public bool HasPermission(TeamRule rule, Permission permission)
        {
            if (_permissionsMap.TryGetValue(rule, out var allowedPermissions))
            {
                return (allowedPermissions & permission) == permission;
            }
            return false;
        }

        public Permission GetPermissions(TeamRule rule)
        {
            if (_permissionsMap.TryGetValue(rule, out var permissions))
            {
                return permissions;
            }
            return Permission.None;
        }
    }
}