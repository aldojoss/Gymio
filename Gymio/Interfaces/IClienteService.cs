using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IClienteService
    {
        Task<bool> RegistrarClienteAsync(Cliente nuevoCliente);
        Task<List<Cliente>> ObtenerClientesAsync(string terminoBusqueda = "");

        
        Task<Cliente?> ObtenerClientePorQRAsync(string codigoQR);
        Task<bool> RegistrarAsistenciaAsync(int clienteId, bool accesoPermitido);
    }
}
