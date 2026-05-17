using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymio.Models
{
    public class CompraInventario
    {
        [Key]
        public int Id { get; set; }

 
        [Required]
        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? ProductoReabastecido { get; set; }

        [Required]
        public int CantidadComprada { get; set; }

        
        [Required]
        public int EgresoId { get; set; }
        [ForeignKey("EgresoId")]
        public Egreso? EgresoGenerado { get; set; }

     
        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}