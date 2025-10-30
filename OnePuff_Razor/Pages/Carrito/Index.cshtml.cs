using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;
using System.Security.Claims;
using CarritoEntity = OnePuff_Razor.Models.Carrito;

namespace OnePuff_Razor.Pages.Carrito
{
    [IgnoreAntiforgeryToken] // Para peticiones AJAX
    public class IndexModel : PageModel
    {
        private readonly CarritoService _carritoService;
        private readonly AppDbContext _context;

        public IndexModel(CarritoService carritoService, AppDbContext context)
        {
            _carritoService = carritoService;
            _context = context;
        }
        // POST /Carrito?handler=Finalizar
        public async Task<IActionResult> OnPostFinalizarAsync(
            [FromServices] PedidoService pedidoService,
            [FromServices] EmailService emailService)
        {
            // 1) Obtener ClienteId del usuario logueado (ya tenés este método en la página)
            var clienteId = await GetOrCreateClienteIdAsync();

            // 2) Crear pedido desde el carrito
            var pedido = await pedidoService.CrearDesdeCarritoAsync(clienteId);

            // 3) Enviar ticket por mail (si el usuario tiene email)
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (!string.IsNullOrWhiteSpace(usuario?.Email))
            {
                try
                {
                    await emailService.EnviarTicketAsync(usuario.Email, pedido);
                }
                catch
                {
                    // Podés loguear el error si agregás logger; no frenamos la compra por un problema de email.
                }
            }

            // 4) Responder al fetch
            return new JsonResult(new { success = true, pedidoId = pedido.PedidoId });
        }
        public CarritoEntity CarritoActual { get; set; } = new();

        //  Obtiene o crea el Cliente vinculado al usuario logueado
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

        //  Carga el carrito actual
        public async Task OnGetAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            CarritoActual = await _carritoService.GetCarritoConItems(clienteId);
        }

        //  Agregar producto
        public async Task<IActionResult> OnPostAgregarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.AddItemAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        //  Quitar una unidad
        public async Task<IActionResult> OnPostQuitarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveOneAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        //  Eliminar producto
        public async Task<IActionResult> OnPostEliminarAsync(int productoId)
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.RemoveItemAsync(clienteId, productoId);
            return new JsonResult(new { success = true });
        }

        //  Vaciar carrito
        public async Task<IActionResult> OnPostVaciarAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            await _carritoService.EmptyAsync(clienteId);
            return new JsonResult(new { success = true });
        }

        //  Cantidad total para el badge del header
        public async Task<IActionResult> OnGetCountAsync()
        {
            var clienteId = await GetOrCreateClienteIdAsync();
            var carrito = await _carritoService.GetCarritoConItems(clienteId);
            var count = carrito.Items?.Sum(i => i.Cantidad) ?? 0;
            return new JsonResult(new { count });
        }
    }
}
