using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Services
{
    public class CarritoService
    {
        private readonly AppDbContext _context;

        public CarritoService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Devuelve el carrito del cliente o lo crea si no existe
        public async Task<Carrito> GetOrCreateAsync(int clienteId)
        {
            // Busca el carrito abierto del cliente
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && !c.EstaCerrado);

            // Si no existe, lo crea
            if (carrito == null)
            {
                carrito = new Carrito
                {
                    ClienteId = clienteId,
                    Items = new List<CarritoItem>(), // 👈 se inicializa la lista para evitar null
                    EstaCerrado = false
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            // 👈 Garantiza que siempre haya una lista de Items aunque venga vacía
            carrito.Items ??= new List<CarritoItem>();
            return carrito;
        }

        // ✅ Agregar producto al carrito
        public async Task AddItemAsync(int clienteId, int productoId, int cantidad = 1)
        {
            // Trae o crea el carrito del cliente
            var carrito = await GetOrCreateAsync(clienteId);

            // 🔍 Busca el producto en la BD
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == productoId);
            if (producto == null || !producto.EstaActivo)
                throw new Exception($"❌ Producto con ID {productoId} no encontrado o inactivo.");

            // 🔄 Busca si ya existe ese producto en el carrito
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item == null)
            {
                // ➕ Si no existe, lo agrega como nuevo ítem
                item = new CarritoItem
                {
                    CarritoId = carrito.CarritoId,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitarioSnapshot = producto.Precio
                };

                _context.CarritoItems.Add(item);
            }
            else
            {
                // 🔁 Si ya estaba, aumenta la cantidad
                item.Cantidad += cantidad;
                _context.CarritoItems.Update(item);
            }

            await _context.SaveChangesAsync();
        }

        // ✅ Quitar una unidad
        public async Task RemoveOneAsync(int clienteId, int productoId)
        {
            var carrito = await GetOrCreateAsync(clienteId);
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                item.Cantidad--;
                if (item.Cantidad <= 0)
                    _context.CarritoItems.Remove(item);
                else
                    _context.CarritoItems.Update(item);

                await _context.SaveChangesAsync();
            }
        }

        // ✅ Eliminar el ítem completo
        public async Task RemoveItemAsync(int clienteId, int productoId)
        {
            var carrito = await GetOrCreateAsync(clienteId);
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                _context.CarritoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Vaciar carrito
        public async Task EmptyAsync(int clienteId)
        {
            var carrito = await GetOrCreateAsync(clienteId);
            _context.CarritoItems.RemoveRange(carrito.Items);
            await _context.SaveChangesAsync();
        }

        // ✅ Obtener carrito con productos (para mostrar en vista)
        public async Task<Carrito> GetCarritoConItems(int clienteId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && !c.EstaCerrado);

            // Si no tiene carrito, devuelve uno vacío (evita errores de null)
            return carrito ?? new Carrito { Items = new List<CarritoItem>() };
        }
    }
}
