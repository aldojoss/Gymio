using Microsoft.AspNetCore.SignalR;

namespace Gymio.Hubs
{
    public class ChatHub : Hub
    {
        // Ahora avisamos a todos que un chat específico cambió
        public async Task NotificarNuevoMensaje(string chatId)
        {
            await Clients.All.SendAsync("ChatActualizado", chatId);
        }
    }
}