using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class SuscripcionCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        [Required]
        public int PlanId { get; set; }
        [ForeignKey("PlanId")]
        public Plan Plan { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } // "Activa", "Vencida", "Congelada" aqui meteremos el concepto de cuentas congeladas

        public int DiasCongeladosRestantes { get; set; } = 0;
    }
}
