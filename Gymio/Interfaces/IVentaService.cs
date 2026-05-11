using Gymio.Models;
using Gymio.Services;

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
        Task<List<IngresoMensual>> GetIngresosUltimos6MesesAsync();
        Task<List<decimal>> GetIngresosUltimos7DiasAsync();
    }
}
