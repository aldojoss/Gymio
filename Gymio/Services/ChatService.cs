using MongoDB.Driver;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class ChatService : IChatService
    {
        private readonly IMongoCollection<ChatEntrenador> _chats;

        public ChatService(IMongoDatabase database)
        {
            _chats = database.GetCollection<ChatEntrenador>("Chats");
        }

        public async Task<ChatEntrenador> ObtenerOCrearChatAsync(int clienteId, int entrenadorId)
        {
            // buscamos si ya han hablado antes
            var chatExistente = await _chats.Find(c => c.ClienteId == clienteId && c.EntrenadorId == entrenadorId).FirstOrDefaultAsync();

            if (chatExistente != null)
            {
                return chatExistente; // devolvemos todo el historial
            }

            // si nunca han hablado, creamos la sala de chat vacía
            var nuevoChat = new ChatEntrenador
            {
                ClienteId = clienteId,
                EntrenadorId = entrenadorId,
                FechaUltimoMensaje = DateTime.Now,
                Historial = new List<Mensaje>()
            };

            await _chats.InsertOneAsync(nuevoChat);
            return nuevoChat;
        }

        public async Task EnviarMensajeAsync(string chatId, Mensaje nuevoMensaje)
        {
            // haber esta es una operación de alta eficiencia en MongoDB (Push)
            // osea que en lugar de traer todo el documento, modificarlo y volverlo a guardar,
            // le decimos a la base de datos que pushiee el nuevo mensaje dentro de la lista 'Historial'

            var filtro = Builders<ChatEntrenador>.Filter.Eq(c => c.Id, chatId);

            var actualizacion = Builders<ChatEntrenador>.Update
                .Push(c => c.Historial, nuevoMensaje)
                .Set(c => c.FechaUltimoMensaje, nuevoMensaje.FechaEnvio);

            await _chats.UpdateOneAsync(filtro, actualizacion);
        }
    }
}