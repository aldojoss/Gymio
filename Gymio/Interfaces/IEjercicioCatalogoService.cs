using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IEjercicioCatalogoService
    {
        Task<List<EjercicioCatalogo>> BuscarEjerciciosAsync(string? termino, string? grupoMuscular, string? equipo, int limite = 80);
        Task<List<string>> ObtenerGruposMuscularesAsync();
        Task<List<string>> ObtenerEquiposAsync();
        Task<EjercicioCatalogo?> ObtenerPorIdAsync(string id);
        Task<EjercicioCatalogo?> ResolverVisualPorNombreAsync(string nombre);
        Task<EjercicioCatalogo> CrearEjercicioAsync(EjercicioCatalogo ejercicio);
    }
}
