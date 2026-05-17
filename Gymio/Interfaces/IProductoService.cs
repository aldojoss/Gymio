using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IProductoService
    {
        Task<List<Producto>> ObtenerProductosAsync(string busqueda = "");
        Task CrearProductoAsync(Producto producto);
        Task ActualizarProductoAsync(Producto producto);
        Task<List<Proveedor>> ObtenerProveedoresAsync(string busqueda = "");
        Task GuardarProveedorAsync(Proveedor proveedor);
        Task<List<CompraInventario>> ObtenerComprasInventarioAsync(int cantidad = 50);
        Task<CompraInventario> RegistrarCompraInventarioAsync(int productoId, int cantidad, decimal costoUnitario, int? proveedorId, int usuarioId);
    }
}
