using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnePuff_Razor.Data;
using OnePuff_Razor.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Auth cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Usuarios/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Administrador"));
    options.AddPolicy("ClienteOnly", p => p.RequireRole("Cliente"));
});

// Razor Pages + autorizaciones por página
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Productos/Admin", "AdminOnly");
    options.Conventions.AuthorizePage("/Productos/Cliente", "ClienteOnly");
    
    
     options.Conventions.AuthorizePage("/Pedidos/Cliente", "ClienteOnly");
     options.Conventions.AuthorizePage("/Pedidos/Admin", "AdminOnly");
     options.Conventions.AuthorizePage("/Pedidos/Detalle"); // cualquiera autenticado; la página valida propiedad

    options.Conventions.AuthorizePage("/Usuarios/NuevoAdmin", "AdminOnly");
    options.Conventions.AuthorizePage("/Usuarios/EditarPerfil", "ClienteOnly");

    options.Conventions.AuthorizePage("/Monedero/Recarga", "ClienteOnly");
    options.Conventions.AuthorizePage("/Monedero/Movimientos", "ClienteOnly");

});

// Servicios personalizados
builder.Services.AddScoped<CarritoService>();
builder.Services.AddScoped<PedidoService>();  
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<MonederoService>(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
