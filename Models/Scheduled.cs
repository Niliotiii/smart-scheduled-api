namespace SmartScheduledApi.Models;

public class Scheduled : BaseModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<Assigned> Assigneds { get; set; }
}
