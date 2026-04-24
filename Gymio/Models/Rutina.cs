using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{

  
    public class Rutina
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ClienteId { get; set; } 
        public int EntrenadorId { get; set; }

        public string Titulo { get; set; }
        public string Objetivo { get; set; }// "fuerza hiper, "

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string IndicacionesGenrales {  get; set; }
        public bool Activa { get; set; } 

        public List<DiaEntrenamiento> Dias { get; set; }


    }

    public class DiaEntrenamiento
    {
        public int OrdenDia { get; set; }
        public string NombreDia { get; set; }
        public string ?FocoMuscular { get; set; } // "pecho", "pierna", "fullbody"
        //los ejercicios conectarlo
        public List<EjercicioRutina> Ejercicios { get; set; }

    }

    public class EjercicioRutina
    {

        public string NombreEjercicio { get; set; }
        public int Series { get; set; }
        public string Repeticiones { get; set; }
        public int DescansoSegundos { get; set; }


        public string ?RIR { get; set; } // "2 RIR", "3 RIR", "Al fallo"
        public string ?Tempo { get; set; } // "2-0-1", "3-1-1", "4-0-2"
        public string ?NotasCoach { get; set; } // "Mantener la espalda recta", "Controlar el movimiento en la fase excéntrica"
        public string ?UrlVideoReferencia { get; set; } // "https://www.youtube.com/watch?v=EjemploVideo"
    }
}