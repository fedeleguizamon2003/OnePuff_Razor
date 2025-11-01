namespace OnePuff_Razor.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = default!;

        public DateTime FechaPedido { get; set; } = DateTime.Now;
        public string DireccionEntrega { get; set; } = string.Empty;
        public string TelefonoContacto { get; set; } = string.Empty;

        // Estados esperados: Pendiente, Aprobado, Rechazado
        public string Estado { get; set; } = "Pendiente";

        //  trackeamos cuándo se cambió el estado
        public DateTime? FechaActualizacion { get; set; }

        public decimal Total { get; set; }

        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}
