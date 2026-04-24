using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
  
        public class Venta
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public int UsuarioId { get; set; } // El cajero
            [ForeignKey("UsuarioId")]
            public Usuario Cajero { get; set; }

            // Puede ser nulo porque podrías vender un agua a alguien sin registrarlo como cliente
            public int? ClienteId { get; set; }
            [ForeignKey("ClienteId")]
            public Cliente Cliente { get; set; }

            public DateTime FechaVenta { get; set; } = DateTime.Now;

            [Column(TypeName = "decimal(18,2)")]
            public decimal Total { get; set; }

            [Required]
            [MaxLength(50)]
            public string MetodoPago { get; set; } // "Efectivo", "Tarjeta", "Transferencia"

            public ICollection<VentaDetalle> Detalles { get; set; }
        }
    
}
