namespace OnePuff_Razor.Models
{
    public class CarritoItem
    {
        public int CarritoItemId { get; set; }

        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; } = default!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = default!;

        public int Cantidad { get; set; }

        // Guarda precio del momento (opcional)
        public decimal PrecioUnitarioSnapshot { get; set; }
    }


}
