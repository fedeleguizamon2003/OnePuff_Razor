using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;
using System.ComponentModel.DataAnnotations;

namespace OnePuff_Razor.Services
{
    public class MonederoService
    {
        private readonly AppDbContext _context;
        public MonederoService(AppDbContext context) => _context = context;

        // Trae o crea el monedero del cliente
        public async Task<Monedero> GetOrCreateAsync(int clienteId)
        {
            var mon = await _context.Monederos
                .FirstOrDefaultAsync(m => m.ClienteId == clienteId);

            if (mon == null)
            {
                mon = new Monedero
                {
                    ClienteId = clienteId,
                    Saldo = 0m,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };
                _context.Monederos.Add(mon);
                await _context.SaveChangesAsync(); // NO transacción local
            }

            return mon;
        }

        // === Recarga: sobrecarga con TarjetaFake (valida datos de tarjeta y delega) ===
        public async Task RecargarAsync(int clienteId, decimal monto, OnePuff_Razor.Pages.Monedero.TarjetaFake tarjeta)
        {
            if (tarjeta == null) throw new ValidationException("Datos de tarjeta inválidos.");
            if (string.IsNullOrWhiteSpace(tarjeta.Numero) || tarjeta.Numero.Length != 16 || !tarjeta.Numero.All(char.IsDigit))
                throw new ValidationException("Número de tarjeta inválido.");
            if (string.IsNullOrWhiteSpace(tarjeta.Cvv) || tarjeta.Cvv.Length != 3 || !tarjeta.Cvv.All(char.IsDigit))
                throw new ValidationException("CVV inválido.");
            if (string.IsNullOrWhiteSpace(tarjeta.Vencimiento) || !System.Text.RegularExpressions.Regex.IsMatch(tarjeta.Vencimiento, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                throw new ValidationException("Vencimiento inválido (MM/AA).");

            // Texto de referencia que guardamos en el movimiento
            var ult4 = tarjeta.Numero[^4..];
            var desc = $"Recarga con tarjeta **** **** **** {ult4}";

            await RecargarAsync(clienteId, monto, desc);
        }

        // === Recarga simple (usada por la sobrecarga anterior) ===
        public async Task RecargarAsync(int clienteId, decimal monto, string? descripcion = null)
        {
            if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a cero.");

            var mon = await GetOrCreateAsync(clienteId);

            mon.Saldo += monto;
            mon.FechaActualizacion = DateTime.Now;

            _context.MonederoMovimientos.Add(new MonederoMovimiento
            {
                MonederoId = mon.MonederoId,
                Tipo = "Recarga",
                Monto = monto,
                Descripcion = descripcion ?? "Recarga de monedero",
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync(); // NO transacción local
        }

        // === Débito por compra (se invoca desde el handler DENTRO de una transacción externa) ===
        public async Task DebitarPorCompraAsync(int clienteId, decimal total, int pedidoId)
        {
            if (total <= 0) throw new InvalidOperationException("El total debe ser mayor a cero.");

            // Cargamos con tracking para RowVersion
            var mon = await _context.Monederos
                .FirstOrDefaultAsync(m => m.ClienteId == clienteId);

            if (mon == null)
                throw new InvalidOperationException("El monedero no existe.");

            // Chequeo de saldo
            if (mon.Saldo < total)
                throw new InvalidOperationException("Saldo insuficiente.");

            // Actualizamos saldo
            mon.Saldo -= total;
            mon.FechaActualizacion = DateTime.Now;

            // Registramos movimiento
            _context.MonederoMovimientos.Add(new MonederoMovimiento
            {
                MonederoId = mon.MonederoId,
                Tipo = "DebitoCompra",
                Monto = -total,
                Descripcion = $"Pago del pedido #{pedidoId}",
                Fecha = DateTime.Now,
                PedidoId = pedidoId
            });

            // Guardamos. Si alguien tocó RowVersion, EF lanza DbUpdateConcurrencyException
            await _context.SaveChangesAsync(); // NO transacción local
        }

        // === Listado de movimientos con filtros opcionales (incluye filtro por tipo) ===
        public async Task<List<MonederoMovimiento>> GetMovimientosAsync(
            int clienteId, DateTime? desde = null, DateTime? hasta = null, string? tipo = null)
        {
            var mon = await _context.Monederos
                .FirstOrDefaultAsync(m => m.ClienteId == clienteId);

            if (mon == null) return new List<MonederoMovimiento>();

            var q = _context.MonederoMovimientos
                .Where(x => x.MonederoId == mon.MonederoId);

            if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value);
            if (hasta.HasValue) q = q.Where(x => x.Fecha <= hasta.Value);
            if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(x => x.Tipo == tipo);

            return await q.OrderByDescending(x => x.Fecha).ToListAsync();
        }
    }
}
