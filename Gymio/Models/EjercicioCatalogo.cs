using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class EjercicioCatalogo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string GrupoMuscularPrincipal { get; set; } = string.Empty;
        public List<string> MusculosSecundarios { get; set; } = new();
        public string Equipo { get; set; } = string.Empty;
        public string Nivel { get; set; } = "Principiante";
        public string PatronMovimiento { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;
        public string ErroresComunes { get; set; } = string.Empty;
        public string GifUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string NombreNormalizado { get; set; } = string.Empty;
        public string Fuente { get; set; } = "Gymio";
        public string ExternalId { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
