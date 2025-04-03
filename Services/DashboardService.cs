using SmartScheduledApi.Interfaces;
using SmartScheduledApi.Dtos;

namespace SmartScheduledApi.Services;

public class DashboardService : IDashboardService
{
    public async Task<IEnumerable<DashboardDto>> GetDashboardDataAsync()
    {
        // Implementação do método para obter dados do dashboard
        return new List<DashboardDto>
        {
            new DashboardDto { Id = 1, Name = "Dashboard 1", Description = "Descrição do Dashboard 1" },
            new DashboardDto { Id = 2, Name = "Dashboard 2", Description = "Descrição do Dashboard 2" }
        };
    }
}
