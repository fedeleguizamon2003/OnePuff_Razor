using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context) => _context = context;

        [BindProperty]
        public Producto Producto { get; set; } = new Producto();

        public SelectList CategoriasSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var prod = await _context.Productos.FindAsync(id.Value);
            if (prod == null) return NotFound();

            Producto = prod;

            // Cargar combo
            CategoriasSelectList = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Siempre recargar combo si hay que volver a la página
            CategoriasSelectList = new SelectList(_context.Categorias, "CategoriaId", "Nombre");

            if (!ModelState.IsValid)
                return Page();

            // 🔎 Validación: duplicado dentro de la misma categoría, excluyéndome a mí
            var nombre = (Producto.Nombre ?? string.Empty).Trim().ToLower();
            bool duplicado = await _context.Productos.AnyAsync(p =>
                p.ProductoId != Producto.ProductoId &&
                p.CategoriaId == Producto.CategoriaId &&
                p.Nombre.ToLower() == nombre
            );

            if (duplicado)
            {
                ModelState.AddModelError("Producto.Nombre", "Ya existe otro producto con ese nombre en esta categoría.");
                return Page();
            }

            // Guardar cambios
            _context.Attach(Producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Productos.Any(p => p.ProductoId == Producto.ProductoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("/Productos/Admin");
        }
    }
}
