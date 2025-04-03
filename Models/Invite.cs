using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Models;

public class Invite : BaseModel
{
    public int TeamId { get; set; }
    public Team Team { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int InvitedById { get; set; }
    public User InvitedBy { get; set; }
    public TeamRule TeamRule { get; set; }
    public InviteStatus Status { get; set; } = InviteStatus.Waiting;
}
