using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Carrito
{
    public class ResumenModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CarritoService _carritoService;

        public ResumenModel(AppDbContext context, CarritoService carritoService)
        {
            _context = context;
            _carritoService = carritoService;
        }

        public OnePuff_Razor.Models.Carrito CarritoActual { get; set; } = new();

        private async Task<int> GetOrCreateClienteIdAsync()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
                throw new InvalidOperationException("No se encontró el UsuarioId en los claims.");

            int usuarioId = int.Parse(usuarioIdClaim);

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null)
            {
                cliente = new OnePuff_Razor.Models.Cliente
                {
                    UsuarioId = usuarioId,
                    Telefono = ""
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            return cliente.ClienteId;
        }

        public async Task OnGetAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            CarritoActual = await _carritoService.GetCarritoConItems(clienteId);
            CarritoActual.Items ??= new List<OnePuff_Razor.Models.CarritoItem>();
        }
    }
}
