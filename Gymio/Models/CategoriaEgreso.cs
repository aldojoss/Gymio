using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gymio.Models
{
    public class CategoriaEgreso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Egreso>? Egresos { get; set; }
    }
}