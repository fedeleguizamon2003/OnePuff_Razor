using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Pedidos
{
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _context;
        public AdminModel(AppDbContext context) => _context = context;

        public List<Pedido> Pedidos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Pedidos = await _context.Pedidos
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }
    }
}
