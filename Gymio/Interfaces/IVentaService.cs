using Gymio.DTOs;
using Gymio.Models;
using Gymio.Services;
using static Gymio.Components.Pages.Reportes.Reportes;

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

        Task<ResumenFinancieroDto> GetResumenFinancieroMesActualAsync();
        Task<List<ProductoTop>> GetTopProductosVendidosAsync(int cantidad);
        Task<List<TransaccionFinancieraDto>> GetUltimosMovimientosFinancierosAsync(int cantidad);
    }
}
