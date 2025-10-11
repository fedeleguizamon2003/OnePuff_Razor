using System.ComponentModel.DataAnnotations;

namespace OnePuff_Razor.Models
{
    public class Usuario
    {
        // ↳ En BD la PK se llama UsuarioId
        public int UsuarioId { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // ↳ En BD se guarda el hash (no la contraseña en texto)
        [Required, StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Dni { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Cliente";

        // 1:1 opcional: el Admin puede no tener dirección
        public Direccion? Direccion { get; set; }
    }
}
