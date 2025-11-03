using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using OnePuff_Razor.Services;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Monedero
{
    // Página de listado de movimientos con filtros y saldo actual
    public class MovimientosModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly MonederoService _monederoService;

        public MovimientosModel(AppDbContext context, MonederoService monederoService)
        {
            _context = context;
            _monederoService = monederoService;
        }

        // Datos para la vista
        public decimal SaldoActual { get; set; }
        public List<MonederoMovimiento> Movimientos { get; set; } = new();

        // Filtros GET
        [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }
        [BindProperty(SupportsGet = true)] public string? Tipo { get; set; }

        public async Task OnGetAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();

            // Cargamos saldo actual
            var mon = await _monederoService.GetOrCreateAsync(clienteId);
            SaldoActual = mon.Saldo;

            // Ajuste de rangos: si Hasta viene sin hora, usamos fin del día
            DateTime? hastaFin = null;
            if (Hasta.HasValue)
            {
                var h = Hasta.Value;
                hastaFin = new DateTime(h.Year, h.Month, h.Day, 23, 59, 59);
            }

            Movimientos = await _monederoService.GetMovimientosAsync(
                clienteId,
                desde: Desde,
                hasta: hastaFin,
                tipo: string.IsNullOrWhiteSpace(Tipo) ? null : Tipo
            );
        }

        // Helper para obtener o crear Cliente desde Usuario logueado
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
    }
}
