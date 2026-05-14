using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IProductoService
    {
        Task<List<Producto>> ObtenerProductosAsync(string busqueda = "");
        Task CrearProductoAsync(Producto producto);
        Task ActualizarProductoAsync(Producto producto);
    }
}