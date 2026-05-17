using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IMaquinaService
    {
        Task<List<Maquina>> ObtenerMaquinasAsync(string busqueda = "");
        Task GuardarMaquinaAsync(Maquina maquina);
        Task<List<MantenimientoMaquina>> ObtenerMantenimientosAsync(int cantidad = 80);
        Task<MantenimientoMaquina> RegistrarMantenimientoAsync(int maquinaId, string descripcion, decimal costo, int usuarioId, string nuevoEstado);
    }
}
