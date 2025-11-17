using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using Microsoft.EntityFrameworkCore;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class VerificarModel : PageModel
    {
        private readonly AppDbContext _context;

        public VerificarModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Codigo { get; set; } = string.Empty;

        public void OnGet(string email)
        {
            Email = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToPage("/Usuarios/Login");
            }

            if (usuario.EmailVerificado)
            {
                TempData["Msg"] = "Email ya verificado.";
                return RedirectToPage("/Usuarios/Login");
            }

            if (usuario.CodigoExpira == null || usuario.CodigoExpira < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "El código expiró. Pedí uno nuevo.");
                return Page();
            }

            if (usuario.CodigoVerificacion != Codigo)
            {
                ModelState.AddModelError("", "Código incorrecto.");
                return Page();
            }

            usuario.EmailVerificado = true;
            usuario.CodigoVerificacion = null;
            usuario.CodigoExpira = null;

            await _context.SaveChangesAsync();

            TempData["Msg"] = "Email verificado correctamente.";
            return RedirectToPage("/Usuarios/Login");
        }
    }
}
