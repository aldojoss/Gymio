using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IPlanService
    {
        Task<List<Plan>> ObtenerPlanesAsync();
        Task<bool> CrearPlanAsync(Plan nuevoPlan);

        Task<bool> ActualizarPlanAsync(Plan plan);
    }
}