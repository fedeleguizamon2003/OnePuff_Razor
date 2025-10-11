using System.ComponentModel.DataAnnotations;

namespace OnePuff_Razor.Models
{
    public class Producto
    {
        public int ProductoId { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = "";

        [Range(0, 1_000_000)]
        public decimal Precio { get; set; }

        public int Peso { get; set; }              // gramos
        public string? UrlImagen { get; set; }
        public int Stock { get; set; }
        public bool EstaActivo { get; set; } = true;

        // FK
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
