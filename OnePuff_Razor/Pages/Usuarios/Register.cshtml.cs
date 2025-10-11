using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;

        public RegisterModel(AppDbContext context)
        {
            _context = context;
        }

        // =============================
        // 1️⃣ ViewModel para el formulario
        // =============================
        public class InputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es obligatorio")]
            public string Apellido { get; set; } = string.Empty;

            [Required(ErrorMessage = "El DNI es obligatorio")]
            public string Dni { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [DataType(DataType.Password)]
            public string Contrasena { get; set; } = string.Empty;

            [Required(ErrorMessage = "El rol es obligatorio")]
            public string Rol { get; set; } = "Cliente";

            // Campos de dirección (solo si el rol es Cliente)
            public string? Calle { get; set; }
            public string? Altura { get; set; }
            public string? Localidad { get; set; }
        }

        // =============================
        // 2️⃣ Propiedad que une el form con el ViewModel
        // =============================
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        // =============================
        // 3️⃣ Método POST del registro
        // =============================
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ✅ Generar el hash SHA256 de la contraseña
            string hashed = Sha256Hex(Input.Contrasena);

            // ✅ Crear el objeto Usuario (entidad)
            var usuario = new Usuario
            {
                Nombre = Input.Nombre.Trim(),
                Apellido = Input.Apellido.Trim(),
                Dni = Input.Dni.Trim(),
                Email = Input.Email.Trim().ToLowerInvariant(),
                PasswordHash = hashed,
                Rol = Input.Rol
            };

            // ✅ Si el usuario es Cliente, le agregamos la dirección
            if (Input.Rol == "Cliente")
            {
                usuario.Direccion = new Direccion
                {
                    Calle = Input.Calle ?? string.Empty,
                    Altura = Input.Altura ?? string.Empty,
                    Localidad = Input.Localidad ?? string.Empty
                };
            }

            // ✅ Guardar en la base de datos
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // ✅ Redirigir según rol
            if (usuario.Rol == "Administrador")
                return RedirectToPage("/Productos/Index");
            else
                return RedirectToPage("/Categorias/Index");
        }

        // =============================
        // 4️⃣ Función auxiliar para hashear la contraseña
        // =============================
        private static string Sha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
