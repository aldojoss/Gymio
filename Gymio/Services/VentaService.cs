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

        public async Task CrearVentaAsync(Venta nuevaVenta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Ventas.Add (nuevaVenta);

                foreach (var detalle in nuevaVenta.Detalles)
                {
                    if (detalle.ProductoId.HasValue)
                    {
                        var producto = await _context.Productos.FindAsync(detalle.ProductoId.Value);
                        if (producto !=null)
                        {
                            producto.StockActual -= detalle.Cantidad;
                            _context.Productos.Update(producto);

                        }
                    }

                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<VentaDetalle>> GetDetallesVentaAsync(int ventaId)
        {
            return await _context.VentaDetalles.Where(e =>e.VentaId ==ventaId).ToListAsync();
        }

        public async Task<decimal> GetIngresosHoyAsync()
            => await _context.Ventas.Where(v => v.FechaVenta.Date == DateTime.Today).SumAsync(v => v.Total);

        public async Task<int> GetTotalVentasMesAsync()
            => await _context.Ventas.CountAsync(v => v.FechaVenta.Month == DateTime.Now.Month);

        public async Task<List<Venta>> GetUltimasVentasAsync(int cantidad)
            => await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.FechaVenta).Take(cantidad).ToListAsync();

        public async Task<List<Producto>> ObtenerProductosActivosAsync()
        {
            return await _context.Productos.Where(p => p.Activo&&p.StockActual>0).ToListAsync();
        }
    }
}
