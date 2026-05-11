using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        [Required]
        [MaxLength(50)]
        public string Rol { get; set; } // ya sea "Admin", "Cajero"

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool Activo { get; set; } = true;




        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalarioBase { get; set; }

        [MaxLength(20)]
        public FrecuenciaPago? FrecuenciaPago { get; set; } // "Semanal", "Quincenal", "Mensual"

        // un cajero puede registrar muchas ventas 
        public ICollection<Venta> Ventas { get; set; }
    }

    public enum FrecuenciaPago
    {
        Semanal = 1,
        Quincenal = 2,
        Mensual = 3
    }
}
