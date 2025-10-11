using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePuff_Razor.Models
{
    public class Direccion
    {
        // ↳ En BD la PK se llama DireccionId
        public int DireccionId { get; set; }

        [Required]
        public string Calle { get; set; } = string.Empty;

        [Required]
        public string Altura { get; set; } = string.Empty;

        [Required]
        public string Localidad { get; set; } = string.Empty;

        // FK a Usuario (1:1)
        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;
    }
}
