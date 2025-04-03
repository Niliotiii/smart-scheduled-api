namespace SmartScheduledApi.Dtos;
using SmartScheduledApi.Enums;

public class CreateTeamDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateTeamDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class TeamMemberDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

public class AddTeamMemberDto
{
    public int UserId { get; set; }
    public TeamRule TeamRule { get; set; }
}