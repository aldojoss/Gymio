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
            return await _rutinas.Find(r => r.ClienteId == clienteId)
                .SortByDescending(r => r.Activa)
                .ThenByDescending(r => r.FechaInicio)
                .ToListAsync();
        }

        public async Task<Rutina?> ObtenerRutinaActivaAsync(int clienteId)
        {
            return await _rutinas.Find(r => r.ClienteId == clienteId && r.Activa == true).FirstOrDefaultAsync();
        }

        public async Task CrearRutinaAsync(Rutina rutina)
        {
            if (rutina.Activa)
            {
                await DesactivarRutinasDelClienteAsync(rutina.ClienteId);
            }

            rutina.FechaInicio = rutina.FechaInicio == default ? DateTime.Now : rutina.FechaInicio;
            await _rutinas.InsertOneAsync(rutina);
        }

        public async Task ActualizarRutinaAsync(Rutina rutina)
        {
            if (rutina.Activa)
            {
                await DesactivarRutinasDelClienteAsync(rutina.ClienteId, rutina.Id);
            }

            // reemplaza todo el documento JSON por la versión editada
            await _rutinas.ReplaceOneAsync(r => r.Id == rutina.Id, rutina);
        }

        public async Task EliminarRutinaAsync(string id)
        {
            await _rutinas.DeleteOneAsync(r => r.Id == id);
        }

        public async Task ActivarRutinaAsync(string id, int clienteId)
        {
            await DesactivarRutinasDelClienteAsync(clienteId, id);

            var update = Builders<Rutina>.Update.Set(r => r.Activa, true);
            await _rutinas.UpdateOneAsync(r => r.Id == id && r.ClienteId == clienteId, update);
        }

        private async Task DesactivarRutinasDelClienteAsync(int clienteId, string? exceptoId = null)
        {
            var filtro = Builders<Rutina>.Filter.Eq(r => r.ClienteId, clienteId);

            if (!string.IsNullOrWhiteSpace(exceptoId))
            {
                filtro &= Builders<Rutina>.Filter.Ne(r => r.Id, exceptoId);
            }

            await _rutinas.UpdateManyAsync(filtro, Builders<Rutina>.Update.Set(r => r.Activa, false));
        }
    }
}
