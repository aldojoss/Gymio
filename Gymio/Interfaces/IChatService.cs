using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IChatService
    {
        // buscaocrea un chat
        Task<ChatEntrenador> ObtenerOCrearChatAsync(int clienteId, int entrenadorId);

        //enviar un mensajexd
        Task EnviarMensajeAsync(string chatId, Mensaje nuevoMensaje);
        Task MarcarMensajesComoLeidosAsync(string chatId, string rolLector);
    }
}
