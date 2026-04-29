using MongoDB.Driver;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class ProgresoService : IProgresoService
    {
        private readonly IMongoCollection<ProgresoFisico> _progresos;

        public ProgresoService(IMongoDatabase database)
        {
            // creamos o nos conectamos a la colección "Progresos" en Mongo
            _progresos = database.GetCollection<ProgresoFisico>("Progresos");
        }

        public async Task<List<ProgresoFisico>> ObtenerHistorialClienteAsync(int clienteId)
        {
            // buscmosa todos los registros del cliente y los ordena por fecha descendente
            return await _progresos.Find(p => p.ClienteId == clienteId)
                                   .SortByDescending(p => p.FechaRegistro)
                                   .ToListAsync();
        }

        public async Task AgregarProgresoAsync(ProgresoFisico progreso)
        {
            // mongoDB le asignará automáticamente un ID único al guardarlo
            await _progresos.InsertOneAsync(progreso);
        }

        public async Task EliminarProgresoAsync(string id)
        {
            await _progresos.DeleteOneAsync(p => p.Id == id);
        }
    }
}