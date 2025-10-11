using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;
        public RegisterModel(AppDbContext context) => _context = context;

        [BindProperty] public Usuario Usuario { get; set; } = new Usuario();
        [BindProperty] public Direccion Direccion { get; set; } = new Direccion();

        // 👇 Este es el campo que llena el form (NO usar Usuario.Contraseña)
        [BindProperty] public string Password { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1️⃣ Validar contraseña del formulario
            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError(nameof(Password), "La contraseña es obligatoria.");

            // 2️⃣ Normalizar datos básicos
            Usuario.Dni = Usuario.Dni?.Trim();
            Usuario.Email = Usuario.Email?.Trim().ToLower();

            // 3️⃣ Validar duplicados
            if (_context.Usuarios.Any(u => u.Dni == Usuario.Dni))
                ModelState.AddModelError("Usuario.Dni", "Ya existe un usuario registrado con ese DNI.");
            if (_context.Usuarios.Any(u => u.Email == Usuario.Email))
                ModelState.AddModelError("Usuario.Email", "Ya existe una cuenta con este correo electrónico.");

            // 4️⃣ Si hay errores, volvemos (ignorando PasswordHash por ahora)
            // 👉 Removemos el campo de la validación temporalmente
            ModelState.Remove("Usuario.PasswordHash");

            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState)
                    if (e.Value?.Errors.Count > 0)
                        Console.WriteLine($"❌ {e.Key}: {string.Join(", ", e.Value.Errors.Select(er => er.ErrorMessage))}");

                return Page();
            }

            // 5️⃣ Hash de la contraseña
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(Password);
                var hash = sha.ComputeHash(bytes);
                Usuario.PasswordHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            // 6️⃣ Revalidamos solo el objeto Usuario (ahora con el hash cargado)
            TryValidateModel(Usuario, nameof(Usuario));

            // 7️⃣ Guardamos usuario
            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            // 8️⃣ Si es cliente, guardar dirección vinculada
            if (Usuario.Rol == "Cliente")
            {
                Direccion.UsuarioId = Usuario.UsuarioId;
                _context.Direcciones.Add(Direccion);
                await _context.SaveChangesAsync();
            }

            TempData["RegistroExitoso"] = "✅ Cuenta creada correctamente. Iniciá sesión.";
            return RedirectToPage("/Usuarios/Login");
        }

    }
}
