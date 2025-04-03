using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Dtos;

public class InviteDto
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int InvitedById { get; set; }
    public string InvitedByName { get; set; }
    public TeamRule TeamRule { get; set; }
    public InviteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateInviteDto
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public TeamRule TeamRule { get; set; }
}

public class InviteUserDto
{
    public int UserId { get; set; }
    public TeamRule TeamRule { get; set; }
}


