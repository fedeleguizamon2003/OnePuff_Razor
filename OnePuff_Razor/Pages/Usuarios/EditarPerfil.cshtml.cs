using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Usuarios
{
    // Requiere estar logueado; por convención en Program.cs la limitamos a Cliente (ver snippet abajo).
    [Authorize]
    public class EditarPerfilModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditarPerfilModel(AppDbContext context) => _context = context;

        // ---- Campos solo lectura para mostrar en la vista ----
        public string NombreCompleto { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // ---- Campos editables ----
        [BindProperty]
        [Required, EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Telefono { get; set; } = string.Empty;

        [BindProperty]
        public Direccion Direccion { get; set; } = new Direccion();

        public async Task<IActionResult> OnGetAsync()
        {
            // Id de usuario logueado (claim NameIdentifier)
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
                return Unauthorized();

            int usuarioId = int.Parse(usuarioIdClaim);

            // Traemos Usuario + Direccion + Cliente (para teléfono)
            var usuario = await _context.Usuarios
                .Include(u => u.Direccion)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null) return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            // Nota: si no existe cliente (por ejemplo admin), podrías manejarlo según tu regla.
            // En este flujo apuntamos a Cliente; Program.cs limitará acceso.

            // Poblar datos de vista
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}";
            Dni = usuario.Dni;
            Rol = usuario.Rol;

            Email = usuario.Email;
            Telefono = cliente?.Telefono ?? string.Empty;

            Direccion = usuario.Direccion ?? new Direccion
            {
                UsuarioId = usuario.UsuarioId // importante para crearla si no existe
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validación server-side de Email requerido y formato se hace por data annotations
            if (!ModelState.IsValid) return Page();

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
                return Unauthorized();

            int usuarioId = int.Parse(usuarioIdClaim);

            var usuario = await _context.Usuarios
                .Include(u => u.Direccion)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null) return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null)
            {
                // Si por algún motivo no existe (p.ej. se creó Usuario sin Cliente),
                // lo creamos vacío para poder guardar teléfono.
                cliente = new Cliente { UsuarioId = usuarioId, Telefono = "" };
                _context.Clientes.Add(cliente);
            }

            // Normalizamos email
            var emailNorm = (Email ?? string.Empty).Trim().ToLower();

            // Unicidad de email: no puede existir en otro usuario
            bool emailDuplicado = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == emailNorm && u.UsuarioId != usuarioId);

            if (emailDuplicado)
            {
                ModelState.AddModelError(nameof(Email), "Ya existe una cuenta con ese email.");
                return Page();
            }

            // Actualizamos Usuario
            usuario.Email = emailNorm;

            // Actualizamos teléfono en Cliente
            cliente.Telefono = Telefono?.Trim() ?? string.Empty;

            // Dirección: crear o actualizar
            if (usuario.Direccion == null)
            {
                usuario.Direccion = new Direccion
                {
                    UsuarioId = usuario.UsuarioId,
                    Calle = Direccion.Calle?.Trim() ?? string.Empty,
                    Altura = Direccion.Altura?.Trim() ?? string.Empty,
                    Localidad = Direccion.Localidad?.Trim() ?? string.Empty
                };
                _context.Direcciones.Add(usuario.Direccion);
            }
            else
            {
                usuario.Direccion.Calle = Direccion.Calle?.Trim() ?? string.Empty;
                usuario.Direccion.Altura = Direccion.Altura?.Trim() ?? string.Empty;
                usuario.Direccion.Localidad = Direccion.Localidad?.Trim() ?? string.Empty;
                _context.Direcciones.Update(usuario.Direccion);
            }

            // Guardar todo
            await _context.SaveChangesAsync();

            TempData["PerfilOk"] = " Datos actualizados correctamente.";
            return RedirectToPage(); // recarga la página para mostrar alerta
        }
    }
}
