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

        public async Task<List<IngresoMensual>> GetIngresosUltimos6MesesAsync()
        {
            
            var fechaInicio = DateTime.Today.AddMonths(-5);
            fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, 1); 

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio)
                .ToListAsync();

            var reporte = new List<IngresoMensual>();

            for (int i = 0; i < 6; i++)
            {
                var mesActual = fechaInicio.AddMonths(i);
                var totalMes = ventas
                    .Where(v => v.FechaVenta.Year == mesActual.Year && v.FechaVenta.Month == mesActual.Month)
                    .Sum(v => v.Total);

                reporte.Add(new IngresoMensual
                {
                
                    Mes = mesActual.ToString("MMM yyyy", new System.Globalization.CultureInfo("es-NI")).ToUpper().Replace(".", ""),
                    Total = totalMes
                });
            }

            return reporte;
        }


        public async Task<int> GetTotalVentasMesAsync()
            => await _context.Ventas.CountAsync(v => v.FechaVenta.Month == DateTime.Now.Month);

        public async Task<List<Venta>> GetUltimasVentasAsync(int cantidad)
            => await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.FechaVenta).Take(cantidad).ToListAsync();

        public async Task<List<Producto>> ObtenerProductosActivosAsync()
        {
            return await _context.Productos.Where(p => p.Activo&&p.StockActual>0).ToListAsync();
        }
        public async Task<List<decimal>> GetIngresosUltimos7DiasAsync()
        {
           
            var fechaInicio = DateTime.Today.AddDays(-6);

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio)
                .ToListAsync();

            var ingresosPorDia = new List<decimal>();

            for (int i = 0; i < 7; i++)
            {
                var diaActual = fechaInicio.AddDays(i);
    
                var totalDia = ventas
                    .Where(v => v.FechaVenta.Date == diaActual)
                    .Sum(v => v.Total);

                ingresosPorDia.Add(totalDia);
            }

            return ingresosPorDia;
        }
    }
    public class IngresoMensual
    {
        public string Mes {  get; set; }
        public decimal Total { get; set; }
    }

}
