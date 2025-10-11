using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // 👈 Necesario para usar SelectList
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class CreateModel : PageModel
    {
        private readonly OnePuff_Razor.Data.AppDbContext _context;

        public CreateModel(OnePuff_Razor.Data.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Producto Producto { get; set; } = default!;

        // 👇 Esta propiedad se usará para llenar el combo de categorías
        public SelectList CategoriasLista { get; set; } = default!;

        public IActionResult OnGet()
        {
            // Traemos las categorías desde la base de datos y las cargamos en el combo
            CategoriasLista = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validación de modelo
            if (!ModelState.IsValid)
            {
                // 👇 Si hay error, recargamos la lista para que no se vacíe al volver
                CategoriasLista = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
                return Page();
            }

            // Guardamos el producto nuevo
            _context.Productos.Add(Producto);
            await _context.SaveChangesAsync();

            // Redirigimos al listado
            return RedirectToPage("./Index");
        }
    }
}
