using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    //en este apartado el admin asignara el entrenador a cada cliente
    public class AsignacionEntrenador
    {
        [Key]
        public int Id { get; set; }

        
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        public int EntrenadorId { get; set; }

        [ForeignKey("EntrenadorId")]
        public Usuario? Entrenador { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
    }
}