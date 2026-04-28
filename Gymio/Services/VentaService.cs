using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class VentaService : IVentaService
    {
        private readonly GymioDbContext _context;
        public VentaService(GymioDbContext context) => _context = context;

        public async Task<decimal> GetIngresosHoyAsync()
            => await _context.Ventas.Where(v => v.FechaVenta.Date == DateTime.Today).SumAsync(v => v.Total);

        public async Task<int> GetTotalVentasMesAsync()
            => await _context.Ventas.CountAsync(v => v.FechaVenta.Month == DateTime.Now.Month);

        public async Task<List<Venta>> GetUltimasVentasAsync(int cantidad)
            => await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.FechaVenta).Take(cantidad).ToListAsync();
    }
}
