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

        /// <summary>
        /// Crea un pedido a partir del carrito abierto del cliente.
        /// Ahora valida stock y descuenta dentro de una transacción.
        /// </summary>
        public async Task<Pedido> CrearDesdeCarritoAsync(int clienteId)
        {
            // 1) Traer carrito abierto con productos
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && !c.EstaCerrado);

            if (carrito == null || carrito.Items == null || carrito.Items.Count == 0)
                throw new InvalidOperationException("El carrito está vacío o no existe.");

            // 2) Validación de stock
            foreach (var it in carrito.Items)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.ProductoId == it.ProductoId);

                if (producto == null || !producto.EstaActivo)
                    throw new InvalidOperationException(
                        $"El producto '{it.Producto?.Nombre ?? ("ID " + it.ProductoId)}' ya no está disponible.");

                if (producto.Stock < it.Cantidad)
                    throw new InvalidOperationException(
                        $"No hay suficiente stock de '{producto.Nombre}'. " +
                        $"Stock actual: {producto.Stock}, solicitado: {it.Cantidad}");
            }

            // 3) Descontar stock
            foreach (var it in carrito.Items)
            {
                var producto = await _context.Productos
                    .FirstAsync(p => p.ProductoId == it.ProductoId);

                producto.Stock -= it.Cantidad;
                _context.Productos.Update(producto);
            }

            // 4) Datos del cliente
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Direccion)
                .FirstAsync(c => c.ClienteId == clienteId);

            var direccionEntrega = cliente.Usuario?.Direccion != null
                ? $"{cliente.Usuario.Direccion.Calle} {cliente.Usuario.Direccion.Altura}, {cliente.Usuario.Direccion.Localidad}"
                : "Sin dirección definida";

            // 5) Crear Pedido
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

            foreach (var it in carrito.Items)
            {
                var det = new PedidoDetalle
                {
                    ProductoId = it.ProductoId,
                    Cantidad = it.Cantidad,
                    PrecioUnitario = it.PrecioUnitarioSnapshot
                };

                pedido.Detalles.Add(det);
                pedido.Total += det.Subtotal;
            }

            // Guardar pedido + cerrar carrito
            _context.Pedidos.Add(pedido);
            carrito.EstaCerrado = true;
            _context.Carritos.Update(carrito);

            await _context.SaveChangesAsync();

            return pedido;
        }

    }
}
