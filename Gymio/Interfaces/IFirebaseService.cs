using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IFirebaseService
    {
        Task GuardarNotificacionAsync(NotificacionFirestore notificacion);
        Task EnviarPushAsync(string fcmToken, string titulo, string mensaje);
    }
}
