namespace OnePuff_Razor.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }

        // Relación 1:1 con Usuario
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;

        public string Telefono { get; set; } = string.Empty;
    }

}
