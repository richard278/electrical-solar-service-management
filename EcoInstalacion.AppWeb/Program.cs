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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EcoInstalacion.AppWeb.Options;
using EcoInstalacion.AppWeb.Services.Auth;
using EcoInstalacion.Entity.Seguridad;

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

// Registrar JwtOptions y IJwtTokenService
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Autenticación por cookie + JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Index";           // vista de login
    options.AccessDeniedPath = "/Home/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSection.GetValue<string>("SecretKey");

    var envSecretKey = Environment.GetEnvironmentVariable("Jwt__SecretKey");
    var finalSecretKey = !string.IsNullOrWhiteSpace(envSecretKey) ? envSecretKey : secretKey;

    if (string.IsNullOrWhiteSpace(finalSecretKey) ||
        finalSecretKey == "SET_THIS_VALUE_USING_USER_SECRETS_OR_ENVIRONMENT_VARIABLES")
    {
        throw new InvalidOperationException("JWT SecretKey is not configured. Use Jwt__SecretKey as an environment variable or user secret.");
    }

    if (finalSecretKey.Length < 32)
    {
        throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long.");
    }

    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(finalSecretKey))
    };
});

// Autorización (políticas por rol si quieres)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", p => p.RequireRole(RolesSistema.Administrador));
    options.AddPolicy("SoloTecnico", p => p.RequireRole(RolesSistema.Tecnico));

    options.AddPolicy("ApiAuthenticated", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RolesSistema.Administrador, RolesSistema.Tecnico);
    });

    options.AddPolicy("ApiAdminOnly", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RolesSistema.Administrador);
    });
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
