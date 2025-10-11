namespace OnePuff_Razor.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = "";

        // navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
