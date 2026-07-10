using EcoInstalacion.DAL.EcoInstaladorContext;
using EcoInstalacion.IOC;
using Microsoft.EntityFrameworkCore;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.AppWeb.Utilidades.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

const string FrontendDevelopmentCorsPolicy = "FrontendDevelopment";

// === Servicios de aplicación ===

// 👇 Aquí declaras que usas la licencia Community
QuestPDF.Settings.License = LicenseType.Community;
// Servicios de almacenamiento de imágenes local
builder.Services.AddScoped<ILocalImageStorage, LocalImageStore>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
// DbContext
builder.Services.AddDbContext<EcoInstaladorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

// Inyección de dependencias de la capa BLL/DAL
builder.Services.InyectarDependencia(builder.Configuration);

// Autenticación por cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";           // vista de login
        options.AccessDeniedPath = "/Home/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Autorización (políticas por rol si quieres)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", p => p.RequireRole("Administrador"));
    options.AddPolicy("SoloTecnico", p => p.RequireRole("Técnico"));
});

// AntiForgery: header que usas en fetch/AJAX
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC + filtro global de AntiForgery (para POST/PUT/PATCH/DELETE)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendDevelopmentCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// === Middleware pipeline ===

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // opcional
}

// app.UseHttpsRedirection(); // opcional si aún no lo usas
app.UseStaticFiles();

app.UseRouting();

app.UseCors(FrontendDevelopmentCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // 👉 después de UseRouting y antes de MapControllerRoute

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
