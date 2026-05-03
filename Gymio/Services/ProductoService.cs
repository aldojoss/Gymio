using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class ProductoService : IProductoService
    {
        private readonly GymioDbContext _context;

        public ProductoService(GymioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerProductosAsync(string busqueda = "")
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda));
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task CrearProductoAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarProductoAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }
    }
}