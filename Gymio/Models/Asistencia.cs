using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class Asistencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        public DateTime FechaHoraEntrada { get; set; } = DateTime.Now;

        public bool AccesoPermitido { get; set; } // True = Entró, False = Rebotado por falta de pago
    }
}
