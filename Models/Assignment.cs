namespace SmartScheduledApi.Models;

public class Assignment : BaseModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; }
    public ICollection<Assigned> Assigneds { get; set; }
}
