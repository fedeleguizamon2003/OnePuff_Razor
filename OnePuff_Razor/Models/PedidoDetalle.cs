namespace OnePuff_Razor.Models
{
    public class PedidoDetalle
    {
        public int PedidoDetalleId { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = default!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = default!;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

}
