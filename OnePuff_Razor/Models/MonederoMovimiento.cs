using System.ComponentModel.DataAnnotations.Schema;

namespace OnePuff_Razor.Models
{
    public class MonederoMovimiento
    {
        public int MonederoMovimientoId { get; set; }

        public int MonederoId { get; set; }
        public Monedero Monedero { get; set; } = default!;

        public string Tipo { get; set; } = string.Empty; // Recarga | DebitoCompra | Ajuste

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }               // positivo recarga, negativo débito

        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int? PedidoId { get; set; }               // opcional, para enlazar al pedido
    }
}
