using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();

        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Direccion> Direcciones { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // tipo decimal para SQL Server
            mb.Entity<Producto>().Property(p => p.Precio).HasColumnType("decimal(18,2)");

            // Seed mínimo (opcional)
            mb.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = 1, Nombre = "Bebidas" },
                new Categoria { CategoriaId = 2, Nombre = "Almacén" }
            );

            mb.Entity<Producto>().HasData(
                new Producto { ProductoId = 1, Nombre = "Agua 1.5L", Precio = 1200, Peso = 1500, Stock = 50, EstaActivo = true, CategoriaId = 1 },
                new Producto { ProductoId = 2, Nombre = "Arroz 1kg", Precio = 1800, Peso = 1000, Stock = 30, EstaActivo = true, CategoriaId = 2 }
            );

            mb.Entity<Usuario>()
             .HasOne(u => u.Direccion)
             .WithOne(d => d.Usuario)
             .HasForeignKey<Direccion>(d => d.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
