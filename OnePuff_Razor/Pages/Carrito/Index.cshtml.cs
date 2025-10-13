using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using System.Security.Claims;
using CarritoEntity = OnePuff_Razor.Models.Carrito;

namespace OnePuff_Razor.Pages.Carrito
{
    public class IndexModel : PageModel
    {
        private readonly CarritoService _carritoService;
        private readonly AppDbContext _context;

        public IndexModel(CarritoService carritoService, AppDbContext context)
        {
            _carritoService = carritoService;
            _context = context;
        }

        // Carrito renderizado en la vista
        public CarritoEntity CarritoActual { get; set; } = new();

        // 🔎 Obtiene el ClienteId del usuario logueado; si no existe, lo crea
        private async Task<int> GetOrCreateClienteIdAsync()
        {
            // El Login ya guarda el UsuarioId en el claim NameIdentifier
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
                throw new InvalidOperationException("No se encontró el UsuarioId en los claims.");

            int usuarioId = int.Parse(usuarioIdClaim);

            // ¿Existe Cliente para este Usuario?
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (cliente == null)
            {
                // Lo creamos con datos mínimos (teléfono opcional)
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

        public async Task OnGet()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            CarritoActual = await _carritoService.GetCarritoConItems(clienteId);
        }

        public async Task<IActionResult> OnPostAgregar(int productoId)
        {
            try
            {
                var clienteId = await GetOrCreateClienteIdAsync();
                await _carritoService.AddItemAsync(clienteId, productoId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return new JsonResult(new { success = true });

                return RedirectToPage();
            }
            catch (DbUpdateException dbex)
            {
                // Devuelve la inner exception para ver el FK exacto que falla
                var inner = dbex.InnerException?.Message ?? dbex.Message;
                return BadRequest(new { error = inner });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostQuitarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveOneAsync(clienteId, productoId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveItemAsync(clienteId, productoId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVaciarAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.EmptyAsync(clienteId);
            return RedirectToPage();
        }
    }
}
