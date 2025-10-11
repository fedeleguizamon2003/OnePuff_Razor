using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // 👈 Necesario para SelectList
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class EditModel : PageModel
    {
        private readonly OnePuff_Razor.Data.AppDbContext _context;

        public EditModel(OnePuff_Razor.Data.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Producto Producto { get; set; } = default!;

        // 👇 Lista para llenar el combo de categorías
        public SelectList CategoriasLista { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Validación de parámetro nulo
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el producto que queremos editar
            var producto = await _context.Productos
                .Include(p => p.Categoria) // Incluimos la categoría asociada
                .FirstOrDefaultAsync(m => m.ProductoId == id);

            if (producto == null)
            {
                return NotFound();
            }

            // Asignamos el producto encontrado al modelo
            Producto = producto;

            // Cargamos la lista de categorías para el combo
            CategoriasLista = new SelectList(_context.Categorias, "CategoriaId", "Nombre", Producto.CategoriaId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // 👇 Si falla validación, recargamos la lista de categorías
                CategoriasLista = new SelectList(_context.Categorias, "CategoriaId", "Nombre", Producto.CategoriaId);
                return Page();
            }

            // Marcamos el producto como modificado
            _context.Attach(Producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // Guardamos cambios
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(Producto.ProductoId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Redirigimos al índice de productos
            return RedirectToPage("./Index");
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoId == id);
        }
    }
}
