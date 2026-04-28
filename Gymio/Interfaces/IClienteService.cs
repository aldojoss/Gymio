using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Interfaces
{
    public interface IClienteService
    {
        Task<bool> RegistrarClienteAsync(Cliente nuevoCliente);
        Task<List<Cliente>> ObtenerClientesAsync(string terminoBusqueda = "");

        
        Task<Cliente?> ObtenerClientePorQRAsync(string codigoQR);
        Task<bool> RegistrarAsistenciaAsync(int clienteId, bool accesoPermitido);

        Task<int> CargarCantidadClientes();
        Task<decimal> CantidadIngresosHoy();
        Task<bool> ActualizarClienteAsync(Cliente cliente);
       


    }
}
