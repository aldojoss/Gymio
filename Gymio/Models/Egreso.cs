using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class Egreso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Concepto { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? UsuarioRegistra { get; set; }

        
        public int? ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? ProductoReabastecido { get; set; }

        public int? CantidadComprada { get; set; }
    }
}