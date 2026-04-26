using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface ISuscripcionService
    {
        // Se agregaron los parámetros obligatorios para la tabla Venta
        Task<bool> RegistrarSuscripcionAsync(int clienteId, int planId, int usuarioId, string metodoPago);
        Task<List<SuscripcionCliente>> ObtenerSuscripcionesRecientesAsync();
    }
}