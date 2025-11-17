using System.ComponentModel.DataAnnotations;

namespace OnePuff_Razor.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(20)]
        public string Dni { get; set; } = string.Empty;

        //  Se guarda en BD. NO usar NotMapped acá.
        [Required, StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "Cliente";

        // 1:1 (opcional)
        public Direccion? Direccion { get; set; }

        public bool EmailVerificado { get; set; } = false;

        public string? CodigoVerificacion { get; set; }

        public DateTime? CodigoExpira { get; set; }

    }
}
