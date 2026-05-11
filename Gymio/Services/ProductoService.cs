using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class ProductoService : IProductoService
    {
       private readonly IDbContextFactory<GymioDbContext> _contextFactory;

        public ProductoService(IDbContextFactory<GymioDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Producto>> ObtenerProductosAsync(string busqueda = "")
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda));
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task CrearProductoAsync(Producto producto)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarProductoAsync(Producto producto)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }
    }
}