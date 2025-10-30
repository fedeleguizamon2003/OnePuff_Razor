using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePuff_Razor.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly OnePuff_Razor.Data.AppDbContext _context;

        public IndexModel(OnePuff_Razor.Data.AppDbContext context)
        {
            _context = context;
        }

        // Lista de productos que mostraremos en la vista
        public IList<Producto> Productos { get; set; } = default!;

        public async Task OnGetAsync()
        {
            //  Incluimos la relación con Categoría para mostrar su nombre
            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();
        }
    }
}
