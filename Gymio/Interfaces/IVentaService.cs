using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IVentaService
    {
        Task<decimal> GetIngresosHoyAsync();
        Task<int> GetTotalVentasMesAsync();
        Task<List<Venta>> GetUltimasVentasAsync(int cantidad);
    }
}
