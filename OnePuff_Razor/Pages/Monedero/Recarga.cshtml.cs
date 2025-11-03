using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnePuff_Razor.Pages.Monedero
{

    // DTO de tarjeta “ficticia” para simular validación
    public class TarjetaFake
    {
        public string Numero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Vencimiento { get; set; } = string.Empty; // MM/AA
        public string Cvv { get; set; } = string.Empty;
    }

    // Página para recargar el monedero con tarjeta ficticia
    public class RecargaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly MonederoService _monederoService;

        public RecargaModel(AppDbContext context, MonederoService monederoService)
        {
            _context = context;
            _monederoService = monederoService;
        }

        // Mostramos el saldo actual en la vista
        public decimal SaldoActual { get; set; }

        // Campos del formulario (binding y validaciones simples)
        [BindProperty]
        [Range(1, 1_000_000, ErrorMessage = "El monto debe ser mayor a 0.")]
        public decimal Monto { get; set; }

        [BindProperty, Required, RegularExpression(@"^\d{16}$", ErrorMessage = "Número de tarjeta inválido.")]
        public string Numero { get; set; } = string.Empty;

        [BindProperty, Required, RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Vencimiento inválido (MM/AA).")]
        public string Vencimiento { get; set; } = string.Empty;

        [BindProperty, Required, RegularExpression(@"^\d{3}$", ErrorMessage = "CVV inválido.")]
        public string Cvv { get; set; } = string.Empty;

        [BindProperty, Required]
        public string Nombre { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            var mon = await _monederoService.GetOrCreateAsync(clienteId);
            SaldoActual = mon.Saldo;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validación de modelo del form
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // recargar saldo
                return Page();
            }

            try
            {
                var clienteId = await GetOrCreateClienteIdAsync();

                // Armamos el DTO de tarjeta ficticia que consume el service
                var tarjeta = new TarjetaFake
                {
                    Numero = Numero,
                    Vencimiento = Vencimiento,
                    Cvv = Cvv,
                    Nombre = Nombre
                };

                await _monederoService.RecargarAsync(clienteId, Monto, tarjeta);

                TempData["Ok"] = $" Recargaste $ {Monto:N2} correctamente.";
                return RedirectToPage(); // redirect GET para evitar re-envío del form
            }
            catch (ValidationException ex)
            {
                // Errores de validación del DTO de tarjeta
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "El monedero fue modificado por otro proceso. Probá nuevamente.");
            }
            catch (Exception ex)
            {
                // Cualquier otro error controlado
                TempData["Error"] = "No se pudo completar la recarga: " + ex.Message;
                return RedirectToPage();
            }

            await OnGetAsync();
            return Page();
        }

        // Obtiene o crea el Cliente a partir del Usuario logueado (igual patrón que en otras páginas)
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
