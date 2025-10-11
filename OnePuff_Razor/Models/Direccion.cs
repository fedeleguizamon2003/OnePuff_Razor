using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePuff_Razor.Models
{
    public class Direccion
    {
        public int DireccionId { get; set; }

        [Required]
        public string Calle { get; set; } = string.Empty;

        [Required]
        public string Altura { get; set; } = string.Empty;

        [Required]
        public string Localidad { get; set; } = string.Empty;

        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }

        // 👇 Hacerlo opcional evita el “The Usuario field is required” en ModelState.
        public Usuario? Usuario { get; set; }
    }
}
