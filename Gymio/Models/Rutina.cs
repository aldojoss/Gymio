using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class Rutina
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ClienteId { get; set; } // Enlace con la tabla sql   
        public int EntrenadorId { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty; // "Fuerza", "Hipertrofia", etc.

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string IndicacionesGenerales { get; set; } = string.Empty; 
        public bool Activa { get; set; }

        public List<DiaEntrenamiento> Dias { get; set; } = new();
    }

    public class DiaEntrenamiento
    {
        public int OrdenDia { get; set; }
        public string NombreDia { get; set; } = string.Empty;
        public string? FocoMuscular { get; set; } // "Pecho", "Espalda", "Pierna"

        public List<EjercicioRutina> Ejercicios { get; set; } = new();
    }

    public class EjercicioRutina
    {
        public string? EjercicioCatalogoId { get; set; }
        public string NombreEjercicio { get; set; } = string.Empty;
        public string? GrupoMuscularPrincipal { get; set; }
        public string? Equipo { get; set; }
        public string? Nivel { get; set; }
        public string? InstruccionesTecnicas { get; set; }
        public string? ErroresComunes { get; set; }
        public string? GifUrl { get; set; }
        public int Series { get; set; }
        public string Repeticiones { get; set; } = string.Empty;
        public int DescansoSegundos { get; set; }

        public string? RIR { get; set; } // "2 RIR", "Al fallo"
        public string? Tempo { get; set; } // "3-1-1"
        public string? ObjetivoCarga { get; set; }
        public string? NotasCoach { get; set; }
        public string? UrlVideoReferencia { get; set; }
        public int Orden { get; set; }
    }
}
