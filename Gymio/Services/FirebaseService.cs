using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using Gymio.Interfaces;
using Gymio.Models;

namespace Gymio.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task GuardarNotificacionAsync(NotificacionFirestore notificacion)
        {
            try
            {
                // Obtenemos o creamos la colección "Notificaciones"
                CollectionReference colRef = _firestoreDb.Collection("Notificaciones");
                
                // Guardamos el documento con el ID especificado en el modelo
                DocumentReference docRef = colRef.Document(notificacion.Id);
                await docRef.SetAsync(notificacion);
                
                _logger.LogInformation("Notificación guardada en Firestore exitosamente: {Id}", notificacion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la notificación en Firestore para el usuario {UsuarioId}", notificacion.UsuarioId);
            }
        }

        public async Task EnviarPushAsync(string fcmToken, string titulo, string mensaje)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                {
                    _logger.LogWarning("FCM Token nulo o vacío. No se puede enviar la notificación push.");
                    return;
                }

                var message = new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification()
                    {
                        Title = titulo,
                        Body = mensaje
                    }
                };

                // Enviamos la notificación
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Notificación push enviada exitosamente: {Response}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la notificación push FCM.");
            }
        }
    }
}
