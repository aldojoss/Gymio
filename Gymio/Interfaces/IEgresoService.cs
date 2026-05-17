using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IEgresoService
    {
        Task<List<CategoriaEgreso>> ObtenerCategoriasAsync();
        Task<List<Egreso>> ObtenerEgresosAsync(DateTime? desde, DateTime? hasta, int? categoriaId, string? busqueda, int cantidad);
        Task<Egreso> RegistrarEgresoAsync(Egreso nuevoEgreso);
        Task<decimal> GetTotalEgresosAsync(DateTime desde, DateTime hasta);
    }
}
