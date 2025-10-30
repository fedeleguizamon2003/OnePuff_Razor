using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Services
{
    public class PedidoService
    {
        private readonly AppDbContext _context;

        public PedidoService(AppDbContext context)
        {
            _context = context;
        }

        /// Crea un pedido a partir del carrito abierto del cliente.
        public async Task<Pedido> CrearDesdeCarritoAsync(int clienteId)
        {
            // 1) Traer carrito abierto con productos
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && !c.EstaCerrado);

            if (carrito == null || carrito.Items == null || carrito.Items.Count == 0)
                throw new InvalidOperationException("El carrito está vacío o no existe.");

            // 2) Traer datos del cliente/usuario para ticket/dirección
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Direccion)
                .FirstAsync(c => c.ClienteId == clienteId);

            var direccionEntrega = cliente.Usuario?.Direccion != null
                ? $"{cliente.Usuario.Direccion.Calle} {cliente.Usuario.Direccion.Altura}, {cliente.Usuario.Direccion.Localidad}"
                : "Sin dirección definida";

            // 3) Crear pedido (cabecera)
            var pedido = new Pedido
            {
                ClienteId = clienteId,
                FechaPedido = DateTime.Now,
                DireccionEntrega = direccionEntrega,
                TelefonoContacto = cliente.Telefono ?? "",
                Estado = "Pendiente",
                Total = 0m,
                Detalles = new List<PedidoDetalle>()
            };

            // 4) Crear detalles y calcular total
            foreach (var it in carrito.Items)
            {
                var det = new PedidoDetalle
                {
                    ProductoId = it.ProductoId,
                    Cantidad = it.Cantidad,
                    PrecioUnitario = it.PrecioUnitarioSnapshot
                };
                pedido.Detalles.Add(det);
                pedido.Total += det.PrecioUnitario * det.Cantidad;
            }

            // 5) Guardar pedido y cerrar carrito
            _context.Pedidos.Add(pedido);
            carrito.EstaCerrado = true;           // cerramos el carrito usado
            await _context.SaveChangesAsync();

            return pedido;
        }
    }
}
