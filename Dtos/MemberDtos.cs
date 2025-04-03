namespace SmartScheduledApi.Dtos;
using SmartScheduledApi.Enums;

public class UpdateMemberRoleDto
{
    public TeamRule TeamRule { get; set; }
}

public class TeamMemberResponseDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string RoleName { get; set; }
}

public class MemberInfo
{
    public string? TeamRule { get; set; }
    public int? TeamId { get; set; }
}