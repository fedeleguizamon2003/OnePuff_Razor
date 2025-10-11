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

        public ClienteModel(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Propiedades de filtro (BindProperty permite que Razor lea los valores del formulario)
        [BindProperty(SupportsGet = true)] public string? BuscarNombre { get; set; }
        [BindProperty(SupportsGet = true)] public int? CategoriaId { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? PrecioMin { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? PrecioMax { get; set; }

        // 🔹 Datos cargados
        public List<Producto> Productos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1️⃣ Traer categorías
            Categorias = await _context.Categorias.AsNoTracking().ToListAsync();

            // 2️⃣ Crear la consulta base
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.EstaActivo)
                .AsQueryable();

            // 3️⃣ Aplicar filtros dinámicos
            if (!string.IsNullOrWhiteSpace(BuscarNombre))
                query = query.Where(p => p.Nombre.Contains(BuscarNombre));

            if (CategoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == CategoriaId.Value);

            if (PrecioMin.HasValue)
                query = query.Where(p => p.Precio >= PrecioMin.Value);

            if (PrecioMax.HasValue && PrecioMax > 0)
                query = query.Where(p => p.Precio <= PrecioMax.Value);

            // 4️⃣ Ejecutar la consulta
            Productos = await query
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
}
