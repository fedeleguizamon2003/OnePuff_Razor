namespace OnePuff_Razor.Models
{
    public class Carrito
    {
        public int CarritoId { get; set; }

        // FK al Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = default!;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public bool EstaCerrado { get; set; } = false; // True al confirmar el pedido

        // Navegación a los ítems
        public ICollection<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }

}
