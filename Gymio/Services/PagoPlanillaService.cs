using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class PagoPlanillaService : IPlanillaService
    {
        private readonly GymioDbContext _context;
        public PagoPlanillaService(GymioDbContext context)
        {
            _context = context;
        }
        public async Task<DateTime?> ObtenerUltimaFechaPagoAsync(int entrenadorId)
        {
            var ultimoPago = await _context.PagosPlanilla
                .Where(p => p.EntrenadorId == entrenadorId)
                .OrderByDescending(p => p.PeriodoFin)
                .FirstOrDefaultAsync();
            return ultimoPago?.PeriodoFin;
        }

        public async Task<bool> RegistrarPagoAsync(int entrenadorId, int cajeroId, decimal monto, DateTime inicio, DateTime fin)
        {
            bool existeSolpamiento = await _context.PagosPlanilla
                .AnyAsync(p => p.EntrenadorId == entrenadorId
                && p.PeriodoFin >= inicio);
            if (existeSolpamiento)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoEgreso = new Egreso
                {
                    Concepto = $"Pago de planilla  {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}",
                    Monto = monto,
                    Fecha = DateTime.Now,
                    UsuarioId = cajeroId,
                    CategoriaEgresoId = 1
                };
                _context.Egresos.Add(nuevoEgreso);

                await _context.SaveChangesAsync();

                var nuevoPago = new PagoPlanilla
                {
                    EntrenadorId = entrenadorId,
                    PeriodoInicio = inicio,
                    PeriodoFin = fin,
                    EgresoId = nuevoEgreso.Id
                };

                _context.PagosPlanilla.Add(nuevoPago);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
