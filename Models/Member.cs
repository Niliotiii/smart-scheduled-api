using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Models;

public class Member : BaseModel
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; }
    public TeamRule TeamRule { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
    public InviteStatus InviteStatus { get; set; } = InviteStatus.Waiting;
    public ICollection<Assignment> Assignments { get; set; }
}
