using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Productos
{
    public class ClienteModel : PageModel
    {
        private readonly AppDbContext _context;

        // Productos del catálogo
        public List<Producto> Productos { get; set; } = new();

        // Categorías para filtros
        public List<Categoria> Categorias { get; set; } = new();

        // Cantidades actuales del carrito (ProductoId → Cantidad)
        public Dictionary<int, int> CantidadesPorProducto { get; set; } = new();

        // ====== Filtros ======
        [BindProperty(SupportsGet = true)]
        public string? NombreFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaIdFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMinFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMaxFiltro { get; set; }
        // ======================

        public ClienteModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Cargar categorías
            Categorias = await _context.Categorias.ToListAsync();

            // Query base
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            // ====== Aplicar filtros ======
            if (!string.IsNullOrWhiteSpace(NombreFiltro))
                query = query.Where(p => p.Nombre.Contains(NombreFiltro));

            if (CategoriaIdFiltro.HasValue)
                query = query.Where(p => p.CategoriaId == CategoriaIdFiltro.Value);

            if (PrecioMinFiltro.HasValue && PrecioMinFiltro > 0)
                query = query.Where(p => p.Precio >= PrecioMinFiltro.Value);

            if (PrecioMaxFiltro.HasValue && PrecioMaxFiltro > 0)
                query = query.Where(p => p.Precio <= PrecioMaxFiltro.Value);
            // ==============================

            Productos = await query.ToListAsync();

            // ============================================
            // Cargar cantidades del carrito del usuario
            // ============================================
            CantidadesPorProducto = new Dictionary<int, int>();

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Cliente"))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == userId);

                if (cliente != null)
                {
                    var carrito = await _context.Carritos
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.ClienteId == cliente.ClienteId && !c.EstaCerrado);

                    if (carrito?.Items != null)
                    {
                        CantidadesPorProducto = carrito.Items
                            .ToDictionary(i => i.ProductoId, i => i.Cantidad);
                    }
                }
            }
        }
    }
}
