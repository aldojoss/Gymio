using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IRutinaService
    {
        Task<List<Rutina>> ObtenerRutinasPorClienteAsync(int clienteId);
        Task<Rutina?> ObtenerRutinaActivaAsync(int clienteId);
        Task CrearRutinaAsync(Rutina rutina);
        Task ActualizarRutinaAsync(Rutina rutina);
        Task EliminarRutinaAsync(string id);
    }
}