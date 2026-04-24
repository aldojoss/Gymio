using System.ComponentModel.DataAnnotations;

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

        // un cajero puede registrar muchas ventas 
        public ICollection<Venta> Ventas { get; set; }
    }
}
