using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;

var builder = WebApplication.CreateBuilder(args);

// ?? 1) Conexión a la base de datos
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ?? 2) Activar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";        // Si no está logueado, redirige acá
        options.LogoutPath = "/Usuarios/Logout";      // Página de logout
        options.AccessDeniedPath = "/Usuarios/Login"; // Si intenta acceder sin permiso
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddRazorPages(options =>
{
    // ?? Ejemplo: proteger carpeta Productos solo para admin
    options.Conventions.AuthorizeFolder("/Productos", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Categorias", "ClienteOnly");
});

// ?? 3) Políticas de autorización por rol
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("ClienteOnly", policy => policy.RequireRole("Cliente"));
});

var app = builder.Build();

// ?? 4) Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ?? IMPORTANTE: el orden
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
