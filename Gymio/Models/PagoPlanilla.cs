using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class PagoPlanilla
    {
        [Key]
        public int Id { get; set; }

    
       
        [Required]
        public int EntrenadorId { get; set; }
        [ForeignKey("EntrenadorId")]
        public Usuario? Entrenador { get; set; }

        
        [Required]
        public DateTime PeriodoInicio { get; set; }

        [Required]
        public DateTime PeriodoFin { get; set; }


        [Required]
        public int EgresoId { get; set; }
        [ForeignKey("EgresoId")]
        public Egreso? EgresoGenerado { get; set; }
    }
}