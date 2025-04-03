using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Models;

public class Assignment : BaseModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int TeamId { get; set; }  // Adicionado TeamId para associar o Assignment a um time específico
    public Team Team { get; set; }   // Adicionado navegação para o Time
    public ICollection<Assigned> Assigneds { get; set; }
}
