using MongoDB.Driver;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class RutinaService : IRutinaService
    {
        private readonly IMongoCollection<Rutina> _rutinas;

        public RutinaService(IMongoDatabase database)
        {
            //busca la coleccion de rutinas en la base de datos, si no existe, MongoDB la creará automáticamente al insertar el primer documento.
            _rutinas = database.GetCollection<Rutina>("Rutinas");
        }

        public async Task<List<Rutina>> ObtenerRutinasPorClienteAsync(int clienteId)
        {
            return await _rutinas.Find(r => r.ClienteId == clienteId).ToListAsync();
        }

        public async Task<Rutina?> ObtenerRutinaActivaAsync(int clienteId)
        {
            return await _rutinas.Find(r => r.ClienteId == clienteId && r.Activa == true).FirstOrDefaultAsync();
        }

        public async Task CrearRutinaAsync(Rutina rutina)
        {
            await _rutinas.InsertOneAsync(rutina);
        }

        public async Task ActualizarRutinaAsync(Rutina rutina)
        {
            // reemplaza todo el documento JSON por la versión editada
            await _rutinas.ReplaceOneAsync(r => r.Id == rutina.Id, rutina);
        }

        public async Task EliminarRutinaAsync(string id)
        {
            await _rutinas.DeleteOneAsync(r => r.Id == id);
        }
    }
}