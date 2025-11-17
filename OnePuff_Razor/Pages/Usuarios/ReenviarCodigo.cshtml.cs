using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using Microsoft.EntityFrameworkCore;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class ReenviarCodigoModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public ReenviarCodigoModel(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToPage("/Usuarios/Login");
            }

            // Nuevo código
            var random = new Random();
            usuario.CodigoVerificacion = random.Next(100000, 999999).ToString();
            usuario.CodigoExpira = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            // Enviar email
            var emailService = new EmailService(_config);
            var cuerpo = $@"
                <h3>Nuevo código</h3>
                <p>Tu nuevo código es:</p>
                <h2><b>{usuario.CodigoVerificacion}</b></h2>";

            await emailService.EnviarFacturaAsync(usuario.Email, cuerpo, true);

            TempData["Msg"] = "Nuevo código enviado.";
            return RedirectToPage("/Usuarios/Verificar", new { email });
        }
    }
}
