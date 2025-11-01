using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) => _context = context;

        // Mostrar algunos productos activos como “destacados”
        public List<Producto> Destacados { get; set; } = new();

        public async Task OnGetAsync()
        {
            Destacados = await _context.Productos
                .Where(p => p.EstaActivo)
                .OrderBy(p => p.Nombre)
                .Take(6)
                .ToListAsync();
        }
    }
}
