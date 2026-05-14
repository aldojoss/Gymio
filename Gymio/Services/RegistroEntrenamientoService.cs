using MongoDB.Driver;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class RegistroEntrenamientoService : IRegistroEntrenamientoService
    {
        private readonly IMongoCollection<RegistroEntrenamiento> _registros;

        public RegistroEntrenamientoService(IMongoDatabase database)
        {
            // aqui se crea la coleccion sola con el primer insert
            _registros = database.GetCollection<RegistroEntrenamiento>("RegistrosEntrenamientos");
        }

        public async Task CrearRegistroAsync(RegistroEntrenamiento registro)
        {
            await _registros.InsertOneAsync(registro);
        }

        public async Task<List<RegistroEntrenamiento>> ObtenerPorClienteAsync(int clienteId)
        {
            return await _registros.Find(r => r.ClienteId == clienteId)
                                   .SortByDescending(r => r.FechaEntrenamiento)
                                   .ToListAsync();
        }
    }
}