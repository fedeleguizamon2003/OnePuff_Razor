using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using Microsoft.AspNetCore.Mvc;

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

        // ? Aprobar
        public async Task<IActionResult> OnPostAprobarAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            if (pedido.Estado == "Aprobado")
            {
                TempData["PedidosMsg"] = $"<b>Pedido #{id}</b> ya estaba aprobado.";
                return RedirectToPage();
            }

            pedido.Estado = "Aprobado";
            pedido.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["PedidosMsg"] = $"? <b>Pedido #{id}</b> aprobado correctamente.";
            return RedirectToPage();
        }

        // ? Rechazar
        public async Task<IActionResult> OnPostRechazarAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            if (pedido.Estado == "Rechazado")
            {
                TempData["PedidosMsg"] = $"<b>Pedido #{id}</b> ya estaba rechazado.";
                return RedirectToPage();
            }

            pedido.Estado = "Rechazado";
            pedido.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["PedidosMsg"] = $"? <b>Pedido #{id}</b> rechazado correctamente.";
            return RedirectToPage();
        }
    }
}
