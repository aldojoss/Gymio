using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Interfaces
{
    public interface IClienteService
    {
        Task<bool> RegistrarClienteAsync(Cliente nuevoCliente);
        Task<List<Cliente>> ObtenerClientesAsync(string terminoBusqueda = "");

        
        Task<Cliente?> ObtenerClientePorQRAsync(string codigoQR);
        Task<List<Cliente>> ObtenerClientesPorEntrenadorAsync(int entrenadorId);
        Task<bool> RegistrarAsistenciaAsync(int clienteId, bool accesoPermitido);

        Task<int> CargarCantidadClientes();
        Task<decimal> CantidadIngresosHoy();
        Task<bool> ActualizarClienteAsync(Cliente cliente);

        Task<int> ObtenerIdEntrenadorAsignadoAsync(int clienteId);
        Task<bool> AsignarEntrenadorAsync(int clienteId, int entrenadorId);
        Task<List<Usuario>> ObtenerEntrenadoresAsync();
        Task<List<Cliente>> BuscarClientesParaSuscripcionAsync(string termino);

        Task<int> GetTotalClientesAsync();
        Task<int> GetTotalClientesActivosAsync();



    }
}
