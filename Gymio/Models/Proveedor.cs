using System.ComponentModel.DataAnnotations;

namespace Gymio.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(100)]
        public string NombreEmpresa { get; set; }

        [StringLength(30)]
        public string? Ruc { get; set; } // Opcional, para la facturación formal

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        // Relación: Un proveedor tiene muchas compras de inventario
        public ICollection<CompraInventario> Compras { get; set; } = new List<CompraInventario>();
    }
}