using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class Plan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // "Mensualidad General" quincena etc

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int DuracionDias { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<SuscripcionCliente> Suscripciones { get; set; }
    }
}
