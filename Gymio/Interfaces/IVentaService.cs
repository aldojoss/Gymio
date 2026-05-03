using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IVentaService
    {
        Task<decimal> GetIngresosHoyAsync();
        Task<int> GetTotalVentasMesAsync();
        Task<List<Venta>> GetUltimasVentasAsync(int cantidad);
        Task<List<VentaDetalle>> GetDetallesVentaAsync(int ventaId);
        Task<List<Producto>> ObtenerProductosActivosAsync();
        Task CrearVentaAsync(Venta nuevaVenta);
    }
}
