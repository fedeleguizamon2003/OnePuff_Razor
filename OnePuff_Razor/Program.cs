using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 2) Cookies de autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Usuarios/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// 3) Autorización y convenciones
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Administrador"));
    options.AddPolicy("ClienteOnly", p => p.RequireRole("Cliente"));
});

builder.Services.AddRazorPages(options =>
{
    // ❌ NO bloquear toda la carpeta /Productos como AdminOnly.
    // ✅ Autorizar por página específica:
    options.Conventions.AuthorizePage("/Productos/Admin", "AdminOnly");
    options.Conventions.AuthorizePage("/Productos/Cliente", "ClienteOnly");
});
// 4) Servicios personalizados
builder.Services.AddScoped<CarritoService>(); 
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Orden correcto
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
