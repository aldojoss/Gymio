using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class MantenimientoMaquina
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaquinaId { get; set; }
        public Maquina Maquina { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Costo { get; set; }

        [Required]
        [StringLength(300)]
        public string Descripcion { get; set; } 

    
        public int? EgresoId { get; set; }
        public Egreso? EgresoAsociado { get; set; }
    }
}