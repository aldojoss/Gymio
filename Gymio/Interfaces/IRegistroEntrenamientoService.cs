using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IRegistroEntrenamientoService
    {
        Task CrearRegistroAsync(RegistroEntrenamiento registro);
        Task<List<RegistroEntrenamiento>> ObtenerPorClienteAsync(int clienteId);

    }
}
