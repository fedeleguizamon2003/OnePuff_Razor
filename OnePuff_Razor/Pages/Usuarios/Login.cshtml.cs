using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;

        public LoginModel(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Propiedades bindeadas desde el formulario
        [BindProperty]
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet()
        {
            // Simplemente muestra la página de login
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //  Validación básica del formulario
            if (!ModelState.IsValid)
                return Page();

            //  Normalizamos el email para evitar problemas de mayúsculas/minúsculas
            var emailNorm = Email.Trim().ToLower();

            //  Buscamos el usuario
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == emailNorm);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return Page();
            }

            //  Hasheamos la contraseña ingresada con SHA256
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(Password);
            var hash = sha.ComputeHash(bytes);
            var hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();

            //  Si el hash no coincide con el guardado en la BD
            if (usuario.PasswordHash != hashHex)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return Page();
            }

            //  Autenticación correcta → crear identidad y cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            //  Configuración de la cookie (válida por 60 min)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
            );

            // Redirección según el rol
            if (usuario.Rol == "Administrador")
                return RedirectToPage("/Productos/Admin");

            if (usuario.Rol == "Cliente")
                return RedirectToPage("/Productos/Cliente");

            //  Si no hay rol definido, vuelve al inicio
            return RedirectToPage("/Index");
        }

        //  Cerrar sesión (lo llamás desde Logout.cshtml.cs)
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}
