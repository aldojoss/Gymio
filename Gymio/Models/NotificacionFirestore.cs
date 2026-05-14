using Google.Cloud.Firestore;

namespace Gymio.Models
{
    [FirestoreData]
    public class NotificacionFirestore
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public int UsuarioId { get; set; }

        [FirestoreProperty]
        public string Titulo { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Mensaje { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public string Tipo { get; set; } = "Alerta"; // Alerta, Pago, Entrenamiento

        [FirestoreProperty]
        public bool Leido { get; set; } = false;
    }
}
