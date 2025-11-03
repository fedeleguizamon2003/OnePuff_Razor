using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePuff_Razor.Models
{
    public class Monedero
    {
        public int MonederoId { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }

        [Timestamp]                 // RowVersion para control de concurrencia
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<MonederoMovimiento> Movimientos { get; set; } = new List<MonederoMovimiento>();
    }
}
