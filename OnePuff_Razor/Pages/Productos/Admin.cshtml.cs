using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _context;

        public AdminModel(AppDbContext context)
        {
            _context = context;
        }

        // ?? Filtros
        [BindProperty(SupportsGet = true)] public string? BuscarNombre { get; set; }
        [BindProperty(SupportsGet = true)] public int? CategoriaId { get; set; }
        [BindProperty(SupportsGet = true)] public string Estado { get; set; } = "Todos";

        // ?? Datos
        public List<Producto> Productos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        // ?? Mensaje temporal (TempData permite mantenerlo tras redirección)
        [TempData]
        public string? Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            // 1?? Cargar categorías
            Categorias = await _context.Categorias.AsNoTracking().ToListAsync();

            // 2?? Crear consulta base
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            // 3?? Aplicar filtros
            if (!string.IsNullOrWhiteSpace(BuscarNombre))
                query = query.Where(p => p.Nombre.Contains(BuscarNombre));

            if (CategoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == CategoriaId.Value);

            if (Estado == "Activos")
                query = query.Where(p => p.EstaActivo);
            else if (Estado == "Inactivos")
                query = query.Where(p => !p.EstaActivo);

            // 4?? Ejecutar y ordenar resultados
            Productos = await query.OrderBy(p => p.Nombre).ToListAsync();
        }

        // ?? Cambiar estado (Activar / Desactivar)
        public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.EstaActivo = !producto.EstaActivo;
            await _context.SaveChangesAsync();

            // ? Mensaje con íconos Bootstrap y color dinámico
            if (producto.EstaActivo)
            {
                Mensaje = $"<div class='text-success'><i class='bi bi-check-circle-fill'></i> " +
                          $"El producto \"{producto.Nombre}\" fue <b>activado</b> correctamente.</div>";
            }
            else
            {
                Mensaje = $"<div class='text-secondary'><i class='bi bi-slash-circle-fill'></i> " +
                          $"El producto \"{producto.Nombre}\" fue <b>desactivado</b> correctamente.</div>";
            }

            // Redirigir al listado mostrando la alerta
            return RedirectToPage();
        }
    }
}
