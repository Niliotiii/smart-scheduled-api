using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SmartScheduledApi.Services;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RenderController : BaseController
    {
        private readonly IMemberService _memberService;
        private readonly IPermissionService _permissionService;

        public RenderController(
            IMemberService memberService,
            IPermissionService permissionService) : base(permissionService)
        {
            _memberService = memberService;
            _permissionService = permissionService;
        }

        [HttpGet("render")]
        [Authorize]
        public async Task<IActionResult> GetRenderInfo([FromQuery] string? selectedTeam = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appPermissions = await _permissionService.GetApplicationPermissionsAsync(userId);
            var teamPermissions = selectedTeam != null ?
                await _permissionService.GetTeamPermissionsAsync(userId, int.Parse(selectedTeam)) :
                TeamPermission.None;

            var rolePermissions = GetRolePermissionsArray(appPermissions);
            var teamRulePermissions = GetTeamPermissionsArray(teamPermissions);

            return Ok(new
            {
                RolePermissions = rolePermissions,
                TeamRulePermissions = teamRulePermissions,
            });
        }

        private Dictionary<string, bool> GetRolePermissionsArray(ApplicationPermission permissions)
        {
            return new Dictionary<string, bool>
            {
                { nameof(ApplicationPermission.ViewTeams), permissions.HasFlag(ApplicationPermission.ViewTeams) },
                { nameof(ApplicationPermission.CreateTeams), permissions.HasFlag(ApplicationPermission.CreateTeams) },
                { nameof(ApplicationPermission.EditTeams), permissions.HasFlag(ApplicationPermission.EditTeams) },
                { nameof(ApplicationPermission.DeleteTeams), permissions.HasFlag(ApplicationPermission.DeleteTeams) },
                { nameof(ApplicationPermission.ViewUsers), permissions.HasFlag(ApplicationPermission.ViewUsers) },
                { nameof(ApplicationPermission.CreateUsers), permissions.HasFlag(ApplicationPermission.CreateUsers) },
                { nameof(ApplicationPermission.EditUsers), permissions.HasFlag(ApplicationPermission.EditUsers) },
                { nameof(ApplicationPermission.DeleteUsers), permissions.HasFlag(ApplicationPermission.DeleteUsers) },
                { nameof(ApplicationPermission.ViewOwnUser), permissions.HasFlag(ApplicationPermission.ViewOwnUser) },
                { nameof(ApplicationPermission.EditOwnUser), permissions.HasFlag(ApplicationPermission.EditOwnUser) },
                { nameof(ApplicationPermission.ManageOwnInvites), permissions.HasFlag(ApplicationPermission.ManageOwnInvites) },
                { nameof(ApplicationPermission.ViewOwnTeams), permissions.HasFlag(ApplicationPermission.ViewOwnTeams) },
                { nameof(ApplicationPermission.ManageSystem), permissions.HasFlag(ApplicationPermission.ManageSystem) }
            };
        }

        private Dictionary<string, bool> GetTeamPermissionsArray(TeamPermission permissions)
        {
            return new Dictionary<string, bool>
            {
                { nameof(TeamPermission.ViewTeam), permissions.HasFlag(TeamPermission.ViewTeam) },
                { nameof(TeamPermission.EditTeam), permissions.HasFlag(TeamPermission.EditTeam) },
                { nameof(TeamPermission.CreateAssignments), permissions.HasFlag(TeamPermission.CreateAssignments) },
                { nameof(TeamPermission.EditAssignments), permissions.HasFlag(TeamPermission.EditAssignments) },
                { nameof(TeamPermission.ViewAssignments), permissions.HasFlag(TeamPermission.ViewAssignments) },
                { nameof(TeamPermission.ManageInvites), permissions.HasFlag(TeamPermission.ManageInvites) },
                { nameof(TeamPermission.CreateSchedules), permissions.HasFlag(TeamPermission.CreateSchedules) },
                { nameof(TeamPermission.EditSchedules), permissions.HasFlag(TeamPermission.EditSchedules) },
                { nameof(TeamPermission.DeleteSchedules), permissions.HasFlag(TeamPermission.DeleteSchedules) },
                { nameof(TeamPermission.ViewSchedules), permissions.HasFlag(TeamPermission.ViewSchedules) },
                { nameof(TeamPermission.ManageTeamSettings), permissions.HasFlag(TeamPermission.ManageTeamSettings) }
            };
        }
    }
}
