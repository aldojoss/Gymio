using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class TurnoCaja
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; } // El recepcionista que abrió la caja
        public Usuario Usuario { get; set; }

        [Required]
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoCalculado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoRealFisico { get; set; }

        public decimal Diferencia => MontoRealFisico - MontoCalculado; // si da negativo, falta plata

        public bool EstaAbierto { get; set; } = true;

        // las ventas y egresos que ocurrieron durante este turno
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Egreso> Egresos { get; set; } = new List<Egreso>();
    }
}