using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    // ?? Solo los usuarios con rol Cliente pueden acceder a esta página
    [Authorize(Roles = "Cliente")]
    public class ClienteModel : PageModel
    {
        private readonly AppDbContext _context;

        // Inyección del contexto de base de datos
        public ClienteModel(AppDbContext context)
        {
            _context = context;
        }

        // Lista de productos que se mostrará en la vista
        public IList<Producto> Productos { get; set; } = new List<Producto>();

        // GET: se ejecuta al entrar en /Productos/Cliente
        public async Task OnGetAsync()
        {
            // Incluimos la categoría para poder mostrarla en la tabla
            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.EstaActivo)
                .ToListAsync();
        }
    }
}
