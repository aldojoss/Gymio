using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{

    //esto falta mejorarlo esta para mientras
    public class Rutina
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ClienteId { get; set; } 

        public string NombreRutina { get; set; } = string.Empty; // Ej: "hipertrofia - Pecho"

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public List<Ejercicio> Ejercicios { get; set; } = new();
    }

    public class Ejercicio
    {
        public string Nombre { get; set; } = string.Empty;
        public int Series { get; set; }
        public string Repeticiones { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }
}