namespace SmartScheduledApi.Models;

public class Team : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Member> Members { get; set; }
}
