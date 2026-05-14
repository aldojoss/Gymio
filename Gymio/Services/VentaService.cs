using Gymio.Data;
using Gymio.DTOs;
using Gymio.Interfaces;
using Gymio.Models;
using Gymio.DTOs;
using Microsoft.EntityFrameworkCore;


namespace Gymio.Services
{
    public class VentaService : IVentaService
    {
    
        private readonly IDbContextFactory<GymioDbContext> _contextFactory;
        private readonly IFirebaseService _firebaseService;
        
        public VentaService(IDbContextFactory<GymioDbContext> contextFactory, IFirebaseService firebaseService)
        {
      
            _contextFactory = contextFactory;
            _firebaseService = firebaseService;
        }

        public async Task CrearVentaAsync(Venta nuevaVenta)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
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

                try
                {
                    // Notificar al cliente si tiene FcmToken
                    var cliente = await _context.Clientes.FindAsync(nuevaVenta.ClienteId);
                    if (cliente != null && !string.IsNullOrEmpty(cliente.FcmToken))
                    {
                        var titulo = "Pago Exitoso";
                        var mensaje = $"Hemos recibido tu pago de {nuevaVenta.Total:C}. ¡Gracias!";
                        
                        await _firebaseService.EnviarPushAsync(cliente.FcmToken, titulo, mensaje);
                        
                        var notificacion = new NotificacionFirestore
                        {
                            UsuarioId = cliente.Id,
                            Titulo = titulo,
                            Mensaje = mensaje,
                            Tipo = "Pago",
                            FechaEnvio = DateTime.UtcNow
                        };
                        
                        await _firebaseService.GuardarNotificacionAsync(notificacion);
                    }
                }
                catch (Exception)
                {
                    // Evitamos que una falla en Firebase afecte la transacción ya confirmada
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<VentaDetalle>> GetDetallesVentaAsync(int ventaId)
        {
            //se usa el using para asegurar que el contexto se libere correctamente después de su uso, evitando posibles problemas de rendimiento o bloqueos en la base de datos.

            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.VentaDetalles.Where(e =>e.VentaId ==ventaId).ToListAsync();
        }

        public async Task<decimal> GetIngresosHoyAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.Ventas.Where(v => v.FechaVenta.Date == DateTime.Today).SumAsync(v => v.Total);
        }
           
           

        public async Task<List<IngresoMensual>> GetIngresosUltimos6MesesAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

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
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.Ventas.CountAsync(v => v.FechaVenta.Month == DateTime.Now.Month);
        }
           

        public async Task<List<Venta>> GetUltimasVentasAsync(int cantidad)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.FechaVenta).Take(cantidad).ToListAsync();
        }
          

        public async Task<List<Producto>> ObtenerProductosActivosAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.Productos.Where(p => p.Activo&&p.StockActual>0).ToListAsync();
        }
        public async Task<List<decimal>> GetIngresosUltimos7DiasAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

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

        public async Task<ResumenFinancieroDto> GetResumenFinancieroMesActualAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var ingresos = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.FechaVenta <= finMes)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var egresos = await _context.Egresos
                .Where(e => e.Fecha >= inicioMes && e.Fecha <= finMes)
                .SumAsync(e => (decimal?)e.Monto) ?? 0;

            return new ResumenFinancieroDto { Ingresos = ingresos, Egresos = egresos };
        }

        public async Task<List<ProductoTop>> GetTopProductosVendidosAsync(int cantidad)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            return await _context.VentaDetalles
                .GroupBy(vd => vd.Concepto)
                .Select(g => new ProductoTop
                {
                    NombreProducto = g.Key ?? "Producto Desconocido",
                    CantidadVendida = g.Sum(vd => vd.Cantidad)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<TransaccionFinancieraDto>> GetUltimosMovimientosFinancierosAsync(int cantidad)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            // Obtenemos las últimas ventas (Ingresos)
            var ventas = await _context.Ventas
                .OrderByDescending(v => v.FechaVenta)
                .Take(cantidad)
                .Select(v => new TransaccionFinancieraDto
                {
                    Fecha = v.FechaVenta,
                    Concepto = "Venta a cliente / POS",
                    Monto = v.Total,
                    EsIngreso = true
                }).ToListAsync();

            var egresos = await _context.Egresos
                .OrderByDescending(e => e.Fecha)
                .Take(cantidad)
                .Select(e => new TransaccionFinancieraDto
                {
                    Fecha = e.Fecha,
                    Concepto = e.Concepto ?? "Egreso general",
                    Monto = e.Monto,
                    EsIngreso = false
                }).ToListAsync();


            return ventas.Concat(egresos)
                .OrderByDescending(t => t.Fecha)
                .Take(cantidad)
                .ToList();
        }
    }
    public class IngresoMensual
    {
        public string Mes {  get; set; }
        public decimal Total { get; set; }
    }

}
