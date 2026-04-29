using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class ChatEntrenador
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Vinculamos a los usuarios de tu base de datos SQL
        public int ClienteId { get; set; }
        public int EntrenadorId { get; set; }

        public DateTime FechaUltimoMensaje { get; set; }

        // Todo el historial del chat se guarda en esta lista anidada
        public List<Mensaje> Historial { get; set; } = new();
    }

    public class Mensaje
    {
        public int RemitenteId { get; set; }
        public string RolRemitente { get; set; } // "Entrenador" o "Cliente"
        public string Texto { get; set; }
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
        public bool Leido { get; set; } = false;
    }
}