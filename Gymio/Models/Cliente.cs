using System.ComponentModel.DataAnnotations;

namespace Gymio.Models
{
    public class Cliente
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        public string Telefono { get; set; }
        public string Email { get; set; }

        // el código QR que escanearemos en la entrada
        [Required]
        [MaxLength(50)]
        public string CodigoQR { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
        public string? FotoUrl { get; set; }
        public string? PasswordHash { get; set; }


        // establecemos las relaciones, un cliente puede tener muchas suscripciones y asistencias
        public ICollection<SuscripcionCliente> Suscripciones { get; set; }
        public ICollection<Asistencia> Asistencias { get; set; }
    }
}
