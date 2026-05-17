using System.ComponentModel.DataAnnotations;

namespace Gymio.Models
{
    public class Maquina
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CodigoInventario { get; set; } // Ej: CAM-001

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } // Ej: Caminadora ProForm 500

        public DateTime FechaAdquisicion { get; set; }

        // Estados: Activa, En Reparacion, De Baja
        [StringLength(20)]
        public string Estado { get; set; } = "Activa";

        // Relación: Una máquina tiene muchos mantenimientos
        public ICollection<MantenimientoMaquina> Mantenimientos { get; set; } = new List<MantenimientoMaquina>();
    }
}