namespace SmartScheduledApi.Models;

public class Assigned : BaseModel
{
    public int ScheduledId { get; set; }
    public Scheduled Scheduled { get; set; }
    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; }
}
