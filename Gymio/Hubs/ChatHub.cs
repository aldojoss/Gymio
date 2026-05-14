using Microsoft.AspNetCore.SignalR;

namespace Gymio.Hubs
{
    public class ChatHub : Hub
    {
        // notificamos a un chat que cambio, para que los clientes puedan actualizar su vista
        public async Task NotificarNuevoMensaje(string chatId)
        {
            await Clients.All.SendAsync("ChatActualizado", chatId);
        }
    }
}