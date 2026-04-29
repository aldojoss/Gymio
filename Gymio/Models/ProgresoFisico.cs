using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gymio.Models
{
    public class ProgresoFisico
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ClienteId { get; set; }
        public DateTime FechaRegistro { get; set; }

        public double PesoLibras { get; set; }
        public double PorcentajeGrasa { get; set; }

        // medidas musculares (pueden ser en cm o pulgadas)
        public double Pecho { get; set; }
        public double Espalda { get; set; }
        public double BrazoDerecho { get; set; }
        public double BrazoIzquierdo { get; set; }
        public double PiernaDerecha { get; set; }
        public double PiernaIzquierda { get; set; }

        // para subir una foto del "Check-in" mensual
        public string? FotoProgresoUrl { get; set; }
    }
}