using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using System.Security.Claims;
using CarritoEntity = OnePuff_Razor.Models.Carrito;

namespace OnePuff_Razor.Pages.Carrito
{
    [IgnoreAntiforgeryToken] // Peticiones AJAX
    public class IndexModel : PageModel
    {
        private readonly CarritoService _carritoService;
        private readonly AppDbContext _context;

        public IndexModel(CarritoService carritoService, AppDbContext context)
        {
            _carritoService = carritoService;
            _context = context;
        }

        public CarritoEntity CarritoActual { get; set; } = new();

        // Carga el carrito actual
        public async Task OnGetAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            CarritoActual = await _carritoService.GetCarritoConItems(clienteId);
        }

        // Agregar producto
        public async Task<IActionResult> OnPostAgregarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.AddItemAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        // Quitar una unidad
        public async Task<IActionResult> OnPostQuitarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveOneAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        // Eliminar producto
        public async Task<IActionResult> OnPostEliminarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveItemAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        // Vaciar carrito
        public async Task<IActionResult> OnPostVaciarAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.EmptyAsync(clienteId);
            return new JsonResult(new { success = true });
        }

        // Cantidad total para el badge del header
        public async Task<IActionResult> OnGetCountAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            var carrito = await _carritoService.GetCarritoConItems(clienteId);
            var count = carrito.Items?.Sum(i => i.Cantidad) ?? 0;
            return new JsonResult(new { count });
        }

        // Finalizar compra (flujo “normal” existente)
        public async Task<IActionResult> OnPostFinalizarAsync(
            [FromServices] PedidoService pedidoService,
            [FromServices] EmailService emailService)
        {
            var clienteId = await GetOrCreateClienteIdAsync();

            var pedido = await pedidoService.CrearDesdeCarritoAsync(clienteId);

            // Enviar ticket por mail si hay email
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (!string.IsNullOrWhiteSpace(usuario?.Email))
            {
                try { await emailService.EnviarTicketAsync(usuario.Email, pedido); }
                catch { /* No frenamos por error de email */ }
            }

            return new JsonResult(new { success = true, pedidoId = pedido.PedidoId });
        }

        // pagar con monedero
        public async Task<IActionResult> OnPostPagarMonederoAsync(
            [FromServices] MonederoService monederoService,
            [FromServices] PedidoService pedidoService,
            [FromServices] EmailService emailService)
        {
            var clienteId = await GetOrCreateClienteIdAsync();

            // Traemos carrito e items para calcular total
            var carrito = await _carritoService.GetCarritoConItems(clienteId);
            if (carrito?.Items == null || carrito.Items.Count == 0)
                return BadRequest("El carrito está vacío.");

            var total = carrito.Items.Sum(i => i.PrecioUnitarioSnapshot * i.Cantidad);
            if (total <= 0)
                return BadRequest("El total del carrito no es válido.");

            // Transacción para crear pedido y debitar monedero de forma atómica
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Chequeo de saldo (rápido); el débito real lo valida con RowVersion en el service
                var mon = await monederoService.GetOrCreateAsync(clienteId);
                if (mon.Saldo < total)
                    return BadRequest("Saldo insuficiente en el monedero.");

                // Creamos el pedido desde el carrito (cierra el carrito)
                var pedido = await pedidoService.CrearDesdeCarritoAsync(clienteId);

                // Debitar el monedero referenciando el pedido
                await monederoService.DebitarPorCompraAsync(clienteId, total, pedido.PedidoId);

                await tx.CommitAsync();

                // Enviar ticket por email
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (!string.IsNullOrWhiteSpace(usuario?.Email))
                {
                    try { await emailService.EnviarTicketAsync(usuario.Email, pedido); }
                    catch { /* Ignoramos error de email */ }
                }

                return new JsonResult(new { success = true, pedidoId = pedido.PedidoId });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Colisión de concurrencia (otro proceso tocó el monedero)
                await tx.RollbackAsync();
                return BadRequest("No se pudo completar el pago: el monedero fue modificado. Probá nuevamente.");
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return BadRequest("Error al procesar el pago: " + ex.Message);
            }
        }

        // Obtiene o crea el Cliente vinculado al usuario logueado (mismo patrón que en otras páginas)
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
