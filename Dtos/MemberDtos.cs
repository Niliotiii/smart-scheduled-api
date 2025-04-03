namespace SmartScheduledApi.Dtos;
using SmartScheduledApi.Enums;

public record MemberDto(
    Guid Id,
    Guid UserId,
    Guid TeamId,
    Guid RoleId
);

public record CreateMemberDto(
    Guid UserId,
    Guid TeamId,
    Guid RoleId
);

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