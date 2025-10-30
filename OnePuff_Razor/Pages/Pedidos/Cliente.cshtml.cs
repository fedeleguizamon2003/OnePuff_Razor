using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Pedidos
{
    public class ClienteModel : PageModel
    {
        private readonly AppDbContext _context;
        public ClienteModel(AppDbContext context) => _context = context;

        public List<Pedido> Pedidos { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (cliente == null) return;

            Pedidos = await _context.Pedidos
                .Where(p => p.ClienteId == cliente.ClienteId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }
    }
}
