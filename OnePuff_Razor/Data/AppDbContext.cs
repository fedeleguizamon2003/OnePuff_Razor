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

        public DbSet<Cliente> Clientes { get; set; } = default!;
        public DbSet<Carrito> Carritos { get; set; } = default!;
        public DbSet<CarritoItem> CarritoItems { get; set; } = default!;
        public DbSet<Pedido> Pedidos { get; set; } = default!;
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder mb)
        {
            // tipo decimal para SQL Server
            mb.Entity<Producto>().Property(p => p.Precio).HasColumnType("decimal(18,2)");
            mb.Entity<CarritoItem>().Property(i => i.PrecioUnitarioSnapshot).HasColumnType("decimal(18,2)");
            mb.Entity<Pedido>().Property(p => p.Total).HasColumnType("decimal(18,2)");
            mb.Entity<PedidoDetalle>().Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");

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

            // Cliente 1:1 Usuario
            mb.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Carrito 1:N CarritoItems
            mb.Entity<Carrito>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Carrito)
                .HasForeignKey(i => i.CarritoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pedido 1:N PedidoDetalles
            mb.Entity<Pedido>()
                .HasMany(p => p.Detalles)
                .WithOne(d => d.Pedido)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
