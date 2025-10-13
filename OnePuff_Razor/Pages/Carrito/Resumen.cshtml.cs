using Microsoft.AspNetCore.Mvc.RazorPages;
using OnePuff_Razor.Models;
using OnePuff_Razor.Services;

namespace OnePuff_Razor.Pages.Carrito
{
    public class ResumenModel : PageModel
    {
        // ⚙️ Servicio que maneja la lógica del carrito (inyectado)
        private readonly CarritoService _carritoService;

        // ✅ Usamos el nombre COMPLETO del tipo para evitar conflicto
        // entre la carpeta "Carrito" (namespace) y la clase "Carrito" (modelo).
        public OnePuff_Razor.Models.Carrito CarritoActual { get; set; } = new();

        // 🔹 Constructor que recibe el servicio por inyección de dependencias
        public ResumenModel(CarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        // 🔹 Método GET que se ejecuta cuando se llama a /Carrito/Resumen
        public async Task OnGet()
        {
            // ⚠️ Por ahora, usamos clienteId = 1 hasta conectar con login real
            int clienteId = 1;

            // 📦 Obtenemos el carrito con sus ítems desde la base de datos
            CarritoActual = await _carritoService.GetCarritoConItems(clienteId);
        }
    }
}
