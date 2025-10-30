using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Data;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Pages.Categorias
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Categoria Categoria { get; set; } = new Categoria();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //  Validación general del modelo
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //  Normalizamos el texto (para evitar que "Bebidas" y "bebidas" sean distintos)
            var nombreNormalizado = Categoria.Nombre.Trim().ToLower();

            //  Validamos si ya existe una categoría con el mismo nombre
            bool existe = _context.Categorias
                .Any(c => c.Nombre.ToLower() == nombreNormalizado);

            if (existe)
            {
                //  Agregamos un error al modelo y no guardamos
                ModelState.AddModelError("Categoria.Nombre", "Ya existe una categoría con ese nombre.");
                return Page();
            }

            //  Si todo está bien, guardamos en la BD
            _context.Categorias.Add(Categoria);
            await _context.SaveChangesAsync();

            //  Redirige al listado o al panel del admin
            return RedirectToPage("/Productos/Admin");
        }
    }
}
