using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Usuarios
{
    public class GestionModel : PageModel
    {
        private readonly AppDbContext _context;

        public GestionModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Usuario> Admins { get; set; } = new();
        public List<Usuario> Clientes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Solo el admin maestro puede entrar
            var emailActual = User.Identity?.Name;
            if (emailActual != "calopez@iiconcepcion.edu.ar")
                return Unauthorized();

            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            Admins = usuarios.Where(u => u.Rol == "Administrador").ToList();
            Clientes = usuarios.Where(u => u.Rol == "Cliente").ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var emailActual = User.Identity?.Name;
            if (emailActual != "calopez@iiconcepcion.edu.ar")
                return Unauthorized();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            // No permitir que elimine su propia cuenta
            if (usuario.Email == "calopez@iiconcepcion.edu.ar")
                return BadRequest("No podés eliminar tu propio usuario maestro.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["Msg"] = $"Usuario {usuario.Email} eliminado correctamente.";
            return RedirectToPage();
        }
    }
}
