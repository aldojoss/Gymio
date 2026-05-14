using MongoDB.Driver;
using Gymio.Models;
using Gymio.Interfaces;
using Gymio.Data;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class RutinaService : IRutinaService
    {
        private readonly IMongoCollection<Rutina> _rutinas;
        private readonly IDbContextFactory<GymioDbContext> _contextFactory;
        private readonly IFirebaseService _firebaseService;

        public RutinaService(IMongoDatabase database, IDbContextFactory<GymioDbContext> contextFactory, IFirebaseService firebaseService)
        {
            //busca la coleccion de rutinas en la base de datos, si no existe, MongoDB la creará automáticamente al insertar el primer documento.
            _rutinas = database.GetCollection<Rutina>("Rutinas");
            _contextFactory = contextFactory;
            _firebaseService = firebaseService;
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

            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();
                var cliente = await _context.Clientes.FindAsync(rutina.ClienteId);

                if (cliente != null && !string.IsNullOrEmpty(cliente.FcmToken))
                {
                    var titulo = "Nueva Rutina Asignada";
                    var mensaje = $"Hola {cliente.Nombre}, tu entrenador te ha asignado una nueva rutina: {rutina.Titulo}. ¡A entrenar!";
                    
                    await _firebaseService.EnviarPushAsync(cliente.FcmToken, titulo, mensaje);
                    
                    var notificacion = new NotificacionFirestore
                    {
                        UsuarioId = cliente.Id,
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Tipo = "Entrenamiento",
                        FechaEnvio = DateTime.UtcNow
                    };
                    
                    await _firebaseService.GuardarNotificacionAsync(notificacion);
                }
            }
            catch (Exception)
            {
                // Ignorar error para no interrumpir el flujo de MongoDB
            }
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