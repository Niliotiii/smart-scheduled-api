using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Services
{
    public static class PermissionMapping
    {
        public static readonly Dictionary<ApplicationRole, ApplicationPermission> ApplicationPermissions = new()
        {
            {
                ApplicationRole.Administrator,
                ApplicationPermission.ViewTeams | ApplicationPermission.CreateTeams |
                ApplicationPermission.EditTeams | ApplicationPermission.DeleteTeams |
                ApplicationPermission.ViewUsers | ApplicationPermission.CreateUsers |
                ApplicationPermission.EditUsers | ApplicationPermission.DeleteUsers |
                ApplicationPermission.ViewOwnUser | ApplicationPermission.EditOwnUser |
                ApplicationPermission.ManageOwnInvites | ApplicationPermission.ViewOwnTeams |
                ApplicationPermission.ManageSystem
            },
            {
                ApplicationRole.User,
                ApplicationPermission.ViewOwnUser | ApplicationPermission.EditOwnUser |
                ApplicationPermission.ManageOwnInvites | ApplicationPermission.ViewOwnTeams
            }
        };

        public static readonly Dictionary<TeamRule, TeamPermission> TeamPermissions = new()
        {
            {
                TeamRule.Leader,
                TeamPermission.ViewTeam | TeamPermission.EditTeam |
                TeamPermission.CreateAssignments | TeamPermission.EditAssignments |
                TeamPermission.ViewAssignments | TeamPermission.ManageInvites |
                TeamPermission.CreateSchedules | TeamPermission.EditSchedules |
                TeamPermission.DeleteSchedules | TeamPermission.ViewSchedules |
                TeamPermission.ManageTeamSettings
            },
            {
                TeamRule.Editor,
                TeamPermission.ViewTeam | TeamPermission.EditTeam |
                TeamPermission.CreateAssignments | TeamPermission.EditAssignments |
                TeamPermission.ViewAssignments | TeamPermission.CreateSchedules |
                TeamPermission.EditSchedules | TeamPermission.DeleteSchedules |
                TeamPermission.ViewSchedules
            },
            {
                TeamRule.Viewer,
                TeamPermission.ViewTeam | TeamPermission.ViewAssignments |
                TeamPermission.ViewSchedules
            }
        };
    }
}
