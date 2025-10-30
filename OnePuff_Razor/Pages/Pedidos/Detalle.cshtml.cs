using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Pedidos
{
    public class DetalleModel : PageModel
    {
        private readonly AppDbContext _context;
        public DetalleModel(AppDbContext context) => _context = context;

        public Pedido Pedido { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Traer pedido + detalles + producto
            var q = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto);

            if (User.IsInRole("Administrador"))
            {
                Pedido = await q.FirstOrDefaultAsync(p => p.PedidoId == id);
            }
            else
            {
                // Verificación de propiedad para clientes
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId);
                if (cliente == null) return Unauthorized();

                Pedido = await q.FirstOrDefaultAsync(p => p.PedidoId == id && p.ClienteId == cliente.ClienteId);
            }

            if (Pedido == null) return NotFound();
            return Page();
        }
    }
}
