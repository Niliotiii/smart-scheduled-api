using SmartScheduledApi.Dtos;

namespace SmartScheduledApi.Interfaces;

public interface IDashboardService
{
    // Defina os métodos que o serviço de dashboard deve implementar
    Task<IEnumerable<DashboardDto>> GetDashboardDataAsync();
}
