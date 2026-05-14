using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class RegistroEntrenamiento
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ClienteId { get; set; }
        public string RutinaId { get; set; } // simplemente tomamos como referencia el Id de la rutina que tiene asignada el cliente, para saber qué rutina estaba siguiendo ese día

        public string NombreDiaEntrenado { get; set; } // Ej: "Día de Pierna"
        public DateTime FechaEntrenamiento { get; set; } = DateTime.Now;

        // La lista de lo que sudó hoy que es 
        public List<RegistroEjercicio> EjerciciosHechos { get; set; } = new();

        public string? ComentariosAtleta { get; set; } // "Hoy me sentí débil del estómago o que es toston", etc.
    }

    public class RegistroEjercicio
    {
        public string NombreEjercicio { get; set; }
        public List<RegistroSerie> Series { get; set; } = new();
    }

    public class RegistroSerie
    {
        public int NumeroSerie { get; set; }
        public double PesoLibras { get; set; }
        public int RepeticionesLogradas { get; set; }
        public bool LlegoAlFallo { get; set; }
    }
}