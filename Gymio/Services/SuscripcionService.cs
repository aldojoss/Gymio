using Microsoft.EntityFrameworkCore;
using Gymio.Data;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class SuscripcionService : ISuscripcionService
    {
        private readonly GymioDbContext _context;

        public SuscripcionService(GymioDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarSuscripcionAsync(int clienteId, int planId, int usuarioId, string metodoPago)
        {
            var plan = await _context.Planes.FindAsync(planId);
            if (plan == null) return false;

            // iniciar una transacción para asegurar que o se guarda todo (Suscripción, Venta y Detalle) o no se guarda nada
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // crear el registro de acceso al gimnasio
                var suscripcion = new SuscripcionCliente
                {
                    ClienteId = clienteId,
                    PlanId = planId,
                    FechaInicio = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddDays(plan.DuracionDias),
                    Estado = "Activa",
                    DiasCongeladosRestantes = 0
                };
                _context.SuscripcionesClientes.Add(suscripcion);

                // crear la cabecera de la factura/recibo y su detalle simultáneamente
                var venta = new Venta
                {
                    UsuarioId = usuarioId,
                    ClienteId = clienteId,
                    FechaVenta = DateTime.Now,
                    Total = plan.Precio,
                    MetodoPago = metodoPago,
                    // entity Framework Core insertará automáticamente estos detalles vinculados al VentaId a la tabla VentaDetalle
                    Detalles = new List<VentaDetalle>
                    {
                        new VentaDetalle
                        {
                            Concepto = $"Suscripción: {plan.Nombre}", // "Suscripción: Mensualidad General"
                            Cantidad = 1,
                            PrecioUnitario = plan.Precio,
                            Subtotal = plan.Precio
                        }
                    }
                };
                _context.Ventas.Add(venta);

                // guardar en base de datos y confirmar transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // si algo falla deshace los cambios para no dejar datos corruptos
            }
        }

        public async Task<List<SuscripcionCliente>> ObtenerSuscripcionesRecientesAsync()
        {
            return await _context.SuscripcionesClientes
                .Include(s => s.Cliente)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.FechaVencimiento)
                .OrderByDescending(s => s.FechaInicio)
                .Take(10)
                .ToListAsync();
        }

        public async Task<(bool Permitido, string Mensaje, SuscripcionCliente? Suscripcion)> ValidarAccesoConQrAsync(string codigoQr)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.CodigoQR == codigoQr);
            if (cliente == null)
            {
                return (false, "Código QR desconocido.", null);
            }

            var suscripcion = await _context.SuscripcionesClientes
                .Include(s => s.Plan)
                .Include(s => s.Cliente)
                .Where(s => s.ClienteId == cliente.Id)
                .OrderByDescending(s => s.FechaVencimiento)
                .FirstOrDefaultAsync();

            if (suscripcion == null)
            {
                return (false, "El socio no tiene ninguna suscripción registrada.", null);
            }

            // comparamos las fechas
            if (suscripcion.FechaVencimiento.Date < DateTime.Today)
            {
                if (suscripcion.Estado == "Activa")
                {
                    suscripcion.Estado = "Vencida";
                    await _context.SaveChangesAsync();
                }
                return (false, $"MEMBRESÍA VENCIDA ({suscripcion.FechaVencimiento.ToShortDateString()})", suscripcion);
            }

            return (true, "ACCESO PERMITIDO", suscripcion);
        }
    }
}