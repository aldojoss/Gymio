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

        public async Task<List<Proveedor>> ObtenerProveedoresAsync(string busqueda = "")
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var query = _context.Proveedores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.NombreEmpresa.Contains(busqueda) ||
                    (p.Ruc != null && p.Ruc.Contains(busqueda)) ||
                    (p.Telefono != null && p.Telefono.Contains(busqueda)));
            }

            return await query
                .OrderByDescending(p => p.Activo)
                .ThenBy(p => p.NombreEmpresa)
                .ToListAsync();
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

        public async Task GuardarProveedorAsync(Proveedor proveedor)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            proveedor.NombreEmpresa = proveedor.NombreEmpresa.Trim();

            if (proveedor.Id == 0)
            {
                _context.Proveedores.Add(proveedor);
            }
            else
            {
                _context.Proveedores.Update(proveedor);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<CompraInventario>> ObtenerComprasInventarioAsync(int cantidad = 50)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            return await _context.ComprasInventario
                .Include(c => c.ProductoReabastecido)
                .Include(c => c.Proveedor)
                .Include(c => c.EgresoGenerado)
                .OrderByDescending(c => c.EgresoGenerado != null ? c.EgresoGenerado.Fecha : DateTime.MinValue)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<CompraInventario> RegistrarCompraInventarioAsync(int productoId, int cantidad, decimal costoUnitario, int? proveedorId, int usuarioId)
        {
            if (productoId <= 0)
            {
                throw new InvalidOperationException("Debes seleccionar un producto.");
            }

            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad comprada debe ser mayor que cero.");
            }

            if (costoUnitario <= 0)
            {
                throw new InvalidOperationException("El costo unitario debe ser mayor que cero.");
            }

            if (usuarioId <= 0)
            {
                throw new InvalidOperationException("No se pudo identificar al usuario que registra la compra.");
            }

            using var _context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var producto = await _context.Productos.FindAsync(productoId)
                    ?? throw new InvalidOperationException("El producto seleccionado no existe.");

                var categoriaInventarioId = await _context.CategoriasEgresos
                    .Where(c => c.Nombre == "Inventario")
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                if (categoriaInventarioId == 0)
                {
                    categoriaInventarioId = 3;
                }

                var totalCompra = cantidad * costoUnitario;
                var egreso = new Egreso
                {
                    Concepto = $"Compra inventario: {producto.Nombre} x {cantidad}",
                    Monto = totalCompra,
                    UsuarioId = usuarioId,
                    CategoriaEgresoId = categoriaInventarioId,
                    Fecha = DateTime.Now
                };

                _context.Egresos.Add(egreso);
                await _context.SaveChangesAsync();

                producto.StockActual += cantidad;
                producto.PrecioCompra = costoUnitario;
                _context.Productos.Update(producto);

                var compra = new CompraInventario
                {
                    ProductoId = producto.Id,
                    CantidadComprada = cantidad,
                    ProveedorId = proveedorId > 0 ? proveedorId : null,
                    EgresoId = egreso.Id
                };

                _context.ComprasInventario.Add(compra);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                compra.ProductoReabastecido = producto;
                compra.EgresoGenerado = egreso;
                if (compra.ProveedorId.HasValue)
                {
                    compra.Proveedor = await _context.Proveedores.FindAsync(compra.ProveedorId.Value);
                }

                return compra;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
