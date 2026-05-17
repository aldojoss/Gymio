using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class Egreso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [MaxLength(200)]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

    
        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? UsuarioRegistra { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaEgresoId { get; set; }
        [ForeignKey("CategoriaEgresoId")]
        public CategoriaEgreso? Categoria { get; set; }

        public int? TurnoCajaId { get; set; }
        public TurnoCaja? TurnoCaja { get; set; }
    }
}