using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        public CreateModel(AppDbContext context) => _context = context;

        // Modelo a crear
        [BindProperty]
        public Producto Producto { get; set; } = new Producto();

        //  Fuente de datos para el <select> de Categorías
        public SelectList CategoriasSelectList { get; set; } = default!;

        public void OnGet()
        {
            // Cargar combo de categorías
            CategoriasSelectList = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Volvemos a cargar el combo si hay errores
            CategoriasSelectList = new SelectList(_context.Categorias, "CategoriaId", "Nombre");

            if (!ModelState.IsValid)
                return Page();

            //  Validación: evitar duplicados por categoría (Nombre + CategoriaId)
            var nombre = (Producto.Nombre ?? string.Empty).Trim().ToLower();
            bool existe = _context.Productos.Any(p =>
                p.CategoriaId == Producto.CategoriaId &&
                p.Nombre.ToLower() == nombre
            );

            if (existe)
            {
                ModelState.AddModelError("Producto.Nombre", "Ya existe un producto con ese nombre en esta categoría.");
                return Page();
            }

            _context.Productos.Add(Producto);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Productos/Admin");
        }
    }
}
