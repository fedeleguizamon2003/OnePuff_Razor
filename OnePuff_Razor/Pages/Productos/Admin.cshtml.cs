using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    // ?? Solo los administradores pueden acceder
    [Authorize(Roles = "Administrador")]
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _context;

        public AdminModel(AppDbContext context)
        {
            _context = context;
        }

        // Lista de productos visibles para administración
        public IList<Producto> Productos { get; set; } = new List<Producto>();

        public async Task OnGetAsync()
        {
            // Incluimos categoría para mostrar en la vista
            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();
        }
    }
}
