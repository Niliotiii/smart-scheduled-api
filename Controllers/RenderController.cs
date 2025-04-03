using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartScheduledApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RenderController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public RenderController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet("render")]
        [Authorize]
        public async Task<IActionResult> GetRenderInfo([FromQuery] string? selectedTeam = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != null && selectedTeam != null)
            {
                var memberInfo = await _memberService.GetMemberInfoAsync(int.Parse(userId), int.Parse(selectedTeam));
                var userRule = memberInfo.TeamRule;

                var renderInfo = GetRenderInfoBasedOnRole(userRole, userRule, selectedTeam);
                return Ok(renderInfo);
            } else {

                // Determinar o que renderizar baseado no papel do usuário e na presença de um time
                var renderInfo = GetRenderInfoBasedOnRole(userRole, null, null);
                return Ok(renderInfo);
            }
        }

        private object GetRenderInfoBasedOnRole(string role, string? rule, string? team)
        {
            // Verifica se o usuário é administrador
            if (role == "Administrator" || role == "Admin")
            {
                // Administradores têm acesso mesmo sem um time selecionado
                return new
                {
                    Page = "AdminDashboard",
                    Permissions = "All",
                    Team = team ?? "None",
                    NeedsTeamSelection = string.IsNullOrEmpty(team),
                    Components = new[] { "TeamManagement", "UserManagement", "SystemSettings" }
                };
            }
            else if (role == "User")
            {
                // Para usuários regulares, primeiro verifica se têm time selecionado
                if (string.IsNullOrEmpty(team))
                {
                    // Sem time selecionado, mostrar página de seleção/criação de time
                    return new
                    {
                        Page = "TeamSelectionPage",
                        Permissions = "SelectTeam",
                        Team = "None",
                        NeedsTeamSelection = true,
                        Components = new[] { "TeamSelector", "CreateTeamButton", "TeamRequestForm" }
                    };
                }

                // Se tem time selecionado mas não tem rule, possivelmente aguardando aprovação
                if (string.IsNullOrEmpty(rule))
                {
                    return new
                    {
                        Page = "PendingAccessPage",
                        Permissions = "None",
                        Team = team,
                        NeedsTeamSelection = false,
                        Components = new[] { "WaitingApproval", "ContactAdmin", "ChangeTeamButton" }
                    };
                }

                // Se tem time e rule, determina o que mostrar baseado na rule
                return GetRenderInfoBasedOnRule(rule, team);
            }
            else
            {
                // Papel desconhecido, mostrar página padrão
                return new
                {
                    Page = "Default",
                    Permissions = "None",
                    Team = "None",
                    NeedsTeamSelection = true,
                    Components = new[] { "AccessDenied" }
                };
            }
        }

        private object GetRenderInfoBasedOnRule(string rule, string teamId)
        {
            switch (rule)
            {
                case "Leader":
                    return new
                    {
                        Page = "TeamLeaderDashboard",
                        Permissions = "ManageMembers,ManageSchedules",
                        Team = teamId,
                        NeedsTeamSelection = false,
                        Components = new[] { "MembersList", "SchedulesList", "InviteForm", "TeamSettings", "ChangeTeamButton" }
                    };

                case "Editor":
                    return new
                    {
                        Page = "EditorDashboard",
                        Permissions = "EditSchedules",
                        Team = teamId,
                        NeedsTeamSelection = false,
                        Components = new[] { "SchedulesList", "ScheduleEditor", "ChangeTeamButton" }
                    };

                case "Viewer":
                    return new
                    {
                        Page = "ViewerDashboard",
                        Permissions = "ReadOnly",
                        Team = teamId,
                        NeedsTeamSelection = false,
                        Components = new[] { "SchedulesList", "ChangeTeamButton" }
                    };

                default:
                    return new
                    {
                        Page = "Default",
                        Permissions = "None",
                        Team = teamId,
                        NeedsTeamSelection = false,
                        Components = new[] { "AccessDenied", "ChangeTeamButton" }
                    };
            }
        }
    }
}
