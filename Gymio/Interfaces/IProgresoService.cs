using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IProgresoService
    {
        
        Task<List<ProgresoFisico>> ObtenerHistorialClienteAsync(int clienteId);


        Task AgregarProgresoAsync(ProgresoFisico progreso);


        Task EliminarProgresoAsync(string id);
    }
}