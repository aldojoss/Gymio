using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class EgresoService : IEgresoService
    {
        private readonly IDbContextFactory<GymioDbContext> _contextFactory;

        public EgresoService(IDbContextFactory<GymioDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<CategoriaEgreso>> ObtenerCategoriasAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.CategoriasEgresos
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Egreso>> ObtenerEgresosAsync(DateTime? desde, DateTime? hasta, int? categoriaId, string? busqueda, int cantidad)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Egresos
                .Include(e => e.Categoria)
                .Include(e => e.UsuarioRegistra)
                .Include(e => e.TurnoCaja)
                .AsQueryable();

            if (desde.HasValue)
            {
                var desdeDia = desde.Value.Date;
                query = query.Where(e => e.Fecha >= desdeDia);
            }

            if (hasta.HasValue)
            {
                var hastaExclusivo = hasta.Value.Date.AddDays(1);
                query = query.Where(e => e.Fecha < hastaExclusivo);
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(e => e.CategoriaEgresoId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(e => e.Concepto.Contains(busqueda));
            }

            return await query
                .OrderByDescending(e => e.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<Egreso> RegistrarEgresoAsync(Egreso nuevoEgreso)
        {
            if (nuevoEgreso.Monto <= 0)
            {
                throw new InvalidOperationException("El monto del egreso debe ser mayor que cero.");
            }

            if (nuevoEgreso.UsuarioId <= 0)
            {
                throw new InvalidOperationException("No se pudo identificar al usuario que registra el egreso.");
            }

            if (nuevoEgreso.CategoriaEgresoId <= 0)
            {
                throw new InvalidOperationException("Debes seleccionar una categoría de egreso.");
            }

            using var context = await _contextFactory.CreateDbContextAsync();

            var existeCategoria = await context.CategoriasEgresos.AnyAsync(c => c.Id == nuevoEgreso.CategoriaEgresoId);
            if (!existeCategoria)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }

            nuevoEgreso.Fecha = DateTime.Now;

            context.Egresos.Add(nuevoEgreso);
            await context.SaveChangesAsync();

            return nuevoEgreso;
        }

        public async Task<decimal> GetTotalEgresosAsync(DateTime desde, DateTime hasta)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var hastaExclusivo = hasta.Date.AddDays(1);

            return await context.Egresos
                .Where(e => e.Fecha >= desde.Date && e.Fecha < hastaExclusivo)
                .SumAsync(e => (decimal?)e.Monto) ?? 0;
        }
    }
}
