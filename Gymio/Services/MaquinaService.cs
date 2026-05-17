using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class MaquinaService : IMaquinaService
    {
        private readonly IDbContextFactory<GymioDbContext> _contextFactory;

        public MaquinaService(IDbContextFactory<GymioDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Maquina>> ObtenerMaquinasAsync(string busqueda = "")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Maquinas
                .Include(m => m.Mantenimientos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(m => m.Nombre.Contains(busqueda) || m.CodigoInventario.Contains(busqueda));
            }

            return await query
                .OrderBy(m => m.Estado == "Activa" ? 0 : m.Estado == "En Reparacion" ? 1 : 2)
                .ThenBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task GuardarMaquinaAsync(Maquina maquina)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            maquina.Nombre = maquina.Nombre.Trim();
            maquina.CodigoInventario = maquina.CodigoInventario.Trim();

            if (maquina.FechaAdquisicion == default)
            {
                maquina.FechaAdquisicion = DateTime.Today;
            }

            if (maquina.Id == 0)
            {
                context.Maquinas.Add(maquina);
            }
            else
            {
                context.Maquinas.Update(maquina);
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<MantenimientoMaquina>> ObtenerMantenimientosAsync(int cantidad = 80)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.MantenimientosMaquinas
                .Include(m => m.Maquina)
                .Include(m => m.EgresoAsociado)
                .OrderByDescending(m => m.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<MantenimientoMaquina> RegistrarMantenimientoAsync(int maquinaId, string descripcion, decimal costo, int usuarioId, string nuevoEstado)
        {
            if (maquinaId <= 0)
            {
                throw new InvalidOperationException("Debes seleccionar una maquina.");
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new InvalidOperationException("Describe el mantenimiento realizado.");
            }

            if (costo < 0)
            {
                throw new InvalidOperationException("El costo no puede ser negativo.");
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var maquina = await context.Maquinas.FindAsync(maquinaId)
                    ?? throw new InvalidOperationException("La maquina seleccionada no existe.");

                int? egresoId = null;
                if (costo > 0)
                {
                    if (usuarioId <= 0)
                    {
                        throw new InvalidOperationException("No se pudo identificar al usuario que registra el egreso.");
                    }

                    var categoriaMantenimientoId = await context.CategoriasEgresos
                        .Where(c => c.Nombre == "Mantenimiento")
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync();

                    if (categoriaMantenimientoId == 0)
                    {
                        categoriaMantenimientoId = 4;
                    }

                    var egreso = new Egreso
                    {
                        Concepto = $"Mantenimiento: {maquina.CodigoInventario} - {maquina.Nombre}",
                        Monto = costo,
                        UsuarioId = usuarioId,
                        CategoriaEgresoId = categoriaMantenimientoId,
                        Fecha = DateTime.Now
                    };

                    context.Egresos.Add(egreso);
                    await context.SaveChangesAsync();
                    egresoId = egreso.Id;
                }

                maquina.Estado = string.IsNullOrWhiteSpace(nuevoEstado) ? maquina.Estado : nuevoEstado;
                context.Maquinas.Update(maquina);

                var mantenimiento = new MantenimientoMaquina
                {
                    MaquinaId = maquina.Id,
                    Fecha = DateTime.Now,
                    Costo = costo,
                    Descripcion = descripcion.Trim(),
                    EgresoId = egresoId
                };

                context.MantenimientosMaquinas.Add(mantenimiento);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                mantenimiento.Maquina = maquina;
                return mantenimiento;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
