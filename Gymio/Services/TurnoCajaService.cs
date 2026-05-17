using Microsoft.EntityFrameworkCore;
using Gymio.Models;
using Gymio.Data;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class TurnoCajaService : ITurnoCajaService
    {
        private readonly IDbContextFactory<GymioDbContext> _contextFactory;

        public TurnoCajaService(IDbContextFactory<GymioDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // 1. Cuando el empleado llega en la mañana
        public async Task<TurnoCaja> AbrirTurnoAsync(int usuarioId, decimal montoInicial)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var nuevoTurno = new TurnoCaja
            {
                UsuarioId = usuarioId,
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                EstaAbierto = true
            };

            context.TurnosCaja.Add(nuevoTurno);
            await context.SaveChangesAsync();
            return nuevoTurno;
        }

        // 2. Para saber si el empleado actual tiene la caja abierta
        public async Task<TurnoCaja?> ObtenerTurnoActivoAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TurnosCaja
                .Include(t => t.Ventas)    
                .Include(t => t.Egresos)
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.EstaAbierto);
        }

        // 3. Cuando el empleado se va en la noche 
        public async Task<TurnoCaja> CerrarTurnoAsync(int turnoId, decimal montoFisicoContado)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var turno = await context.TurnosCaja
                .Include(t => t.Ventas)
                .Include(t => t.Egresos)
                .FirstOrDefaultAsync(t => t.Id == turnoId);

            if (turno == null || !turno.EstaAbierto)
                throw new Exception("El turno no existe o ya está cerrado.");

         
            decimal totalVentas = turno.Ventas.Sum(v => v.Total);

            decimal totalEgresos = turno.Egresos.Sum(e => e.Monto);


            turno.MontoCalculado = turno.MontoInicial + totalVentas - totalEgresos;
            turno.MontoRealFisico = montoFisicoContado;
            turno.FechaCierre = DateTime.Now;
            turno.EstaAbierto = false;

            context.TurnosCaja.Update(turno);
            await context.SaveChangesAsync();

            return turno;
        }

        public async Task<List<TurnoCaja>> ObtenerTurnosAbiertosAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TurnosCaja
                .Include(t => t.Usuario)
                .Include(t => t.Ventas)
                .Include(t => t.Egresos)
                .Where(t => t.EstaAbierto)
                .OrderByDescending(t => t.FechaApertura)
                .ToListAsync();
        }

        public async Task<List<TurnoCaja>> ObtenerCierresRecientesAsync(int cantidad)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TurnosCaja
                .Include(t => t.Usuario)
                .Include(t => t.Ventas)
                .Include(t => t.Egresos)
                .Where(t => !t.EstaAbierto)
                .OrderByDescending(t => t.FechaCierre)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
