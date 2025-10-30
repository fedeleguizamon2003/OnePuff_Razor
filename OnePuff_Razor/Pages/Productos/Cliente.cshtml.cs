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

        // Propiedades públicas accesibles desde la vista (.cshtml)
        public List<Producto> Productos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        //  Filtros (inputs del formulario)
        [BindProperty(SupportsGet = true)]
        public string? NombreFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaIdFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMinFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMaxFiltro { get; set; }

        public ClienteModel(AppDbContext context)
        {
            _context = context;
        }

        //  Se ejecuta al cargar la página
        public async Task OnGetAsync()
        {
            //  Cargar categorías
            Categorias = await _context.Categorias.ToListAsync();

            //  Base query de productos
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            // 🔹Aplicar filtros dinámicos
            if (!string.IsNullOrWhiteSpace(NombreFiltro))
                query = query.Where(p => p.Nombre.Contains(NombreFiltro));

            if (CategoriaIdFiltro.HasValue)
                query = query.Where(p => p.CategoriaId == CategoriaIdFiltro.Value);

            if (PrecioMinFiltro.HasValue && PrecioMinFiltro.Value > 0)
                query = query.Where(p => p.Precio >= PrecioMinFiltro.Value);

            if (PrecioMaxFiltro.HasValue && PrecioMaxFiltro.Value > 0)
                query = query.Where(p => p.Precio <= PrecioMaxFiltro.Value);

            //  Ejecutar y guardar resultado
            Productos = await query.ToListAsync();
        }
    }
}
