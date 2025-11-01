using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace OnePuff_Razor.Pages.Usuarios
{
    // Solo ADMIN puede acceder a esta página
    [Authorize(Roles = "Administrador")]
    public class NuevoAdminModel : PageModel
    {
        private readonly AppDbContext _context;
        public NuevoAdminModel(AppDbContext context) => _context = context;

        [BindProperty] public Usuario Usuario { get; set; } = new Usuario { Rol = "Administrador" };

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Normalización
            Usuario.Email = Usuario.Email?.Trim().ToLower();
            Usuario.Dni = Usuario.Dni?.Trim();
            Usuario.Rol = "Administrador"; // blindado del lado servidor

            // Validaciones simples
            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError(nameof(Password), "La contraseña es obligatoria.");

            // Evitar duplicados
            if (_context.Usuarios.Any(u => u.Email == Usuario.Email))
                ModelState.AddModelError("Usuario.Email", "Ya existe una cuenta con ese correo.");
            if (_context.Usuarios.Any(u => u.Dni == Usuario.Dni))
                ModelState.AddModelError("Usuario.Dni", "Ya existe un usuario con ese DNI.");

            // Ignorar momentáneamente PasswordHash en la validación de modelo
            ModelState.Remove("Usuario.PasswordHash");
            if (!ModelState.IsValid) return Page();

            // Hash SHA256 (coherente con tu sistema)
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Password));
            Usuario.PasswordHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // Validar Usuario ya con hash
            TryValidateModel(Usuario, nameof(Usuario));

            // Guardar
            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            TempData["RegistroExitoso"] = $"? Admin \"{Usuario.Nombre}\" creado correctamente.";
            return RedirectToPage("/Productos/Admin");
        }
    }
}
