using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using OnePuff_Razor.Services;
using System.Security.Cryptography;
using System.Text;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public RegisterModel(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _emailService = new EmailService(config);
        }

        [BindProperty] public Usuario Usuario { get; set; } = new Usuario();
        [BindProperty] public Direccion Direccion { get; set; } = new Direccion();

        // Contraseña ingresada en el formulario (NO es el hash)
        [BindProperty] public string Password { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            Usuario.Rol = "Cliente"; // blindar registro público solo a Cliente

            // 1) Validar contraseña del formulario
            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError(nameof(Password), "La contraseña es obligatoria.");

            // 2) Normalizar datos básicos
            Usuario.Dni = Usuario.Dni?.Trim();
            Usuario.Email = Usuario.Email?.Trim().ToLower();

            // 3) Validar duplicados
            if (_context.Usuarios.Any(u => u.Dni == Usuario.Dni))
                ModelState.AddModelError("Usuario.Dni", "Ya existe un usuario registrado con ese DNI.");

            if (_context.Usuarios.Any(u => u.Email == Usuario.Email))
                ModelState.AddModelError("Usuario.Email", "Ya existe una cuenta con este correo electrónico.");

            // Sacamos temporalmente validation del PasswordHash
            ModelState.Remove("Usuario.PasswordHash");

            // Si hay errores → volver
            if (!ModelState.IsValid)
                return Page();

            // 4) Hash de password
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(Password);
                var hash = sha.ComputeHash(bytes);
                Usuario.PasswordHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            // 5) Generar código de verificación (6 dígitos)
            var random = new Random();
            Usuario.CodigoVerificacion = random.Next(100000, 999999).ToString();
            Usuario.CodigoExpira = DateTime.UtcNow.AddMinutes(15);
            Usuario.EmailVerificado = false;

            // 6) Guardamos Usuario
            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            // 7) Guardamos dirección
            if (Usuario.Rol == "Cliente")
            {
                Direccion.UsuarioId = Usuario.UsuarioId;
                _context.Direcciones.Add(Direccion);
                await _context.SaveChangesAsync();
            }

            // 8) Enviar email con código
            var cuerpo = $@"
                <h3>Bienvenido a OnePuff</h3>
                <p>Tu código de verificación es:</p>
                <h2><b>{Usuario.CodigoVerificacion}</b></h2>
                <p>Expira en 15 minutos.</p>";

            await _emailService.EnviarEmailAsync(
                Usuario.Email,
                "Código de verificación - OnePuff",
                cuerpo
            );

            // 9) Redirigir a la página de verificación
            return RedirectToPage("/Usuarios/Verificar", new { email = Usuario.Email });
        }
    }
}
