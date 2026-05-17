using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class ChatEntrenador
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // se vinvulan a la bd
        public int ClienteId { get; set; }
        public int EntrenadorId { get; set; }

        public DateTime FechaUltimoMensaje { get; set; }

        // asi evito errores si el chat esta vacio no se
        public List<Mensaje> Historial { get; set; } = new List<Mensaje>();
    }

    public class Mensaje
    {
        public int RemitenteId { get; set; }
        public string RolRemitente { get; set; } = string.Empty; // "Entrenador" o "Cliente"
        public string Texto { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
        public bool Leido { get; set; } = false;
    }
}
