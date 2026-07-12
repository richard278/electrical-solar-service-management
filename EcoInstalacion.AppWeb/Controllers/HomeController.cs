using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
//using Microsoft.AspNetCore.Hosting; Paquete necesario para IWebHostEnvironment
//using Microsoft.AspNetCore.Http;
//using System.IO;

namespace EcoInstalacion.AppWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;   // ⬅️ inyecta esto

        public HomeController(IUsuarioService usuarioService, IMapper mapper, IWebHostEnvironment env)
        {
            _usuarioService = usuarioService;
            _mapper = mapper;
            _env = env;
        }

        // Ocultamos el Index del BaseController
        [AllowAnonymous]
        public new IActionResult Index() => View();

        public IActionResult Privacy() => View();

        /*Id de usuario leído DESDE LOS CLAIMS de la cookie.
        new oculta la versión que tengas en BaseController.*/
        protected new int UserId
        {
            get
            {
                var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idClaim, out var id))
                    return id;

                // Fallback de seguridad (se puede lanzar excepción si se prefiere)
                return 0;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new VMLogin());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(VMLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var usuario = await _usuarioService.ObtenerPorCredenciales(
                    model.Correo,
                    model.Clave
                );

                // 1) Credenciales
                if (usuario is null)
                {
                    ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                    return View(model);
                }

                // 2) Usuario activo
                if (usuario.EsActivo == false)
                {
                    ModelState.AddModelError(string.Empty, "El usuario está inactivo. Contacte al administrador.");
                    return View(model);
                }

                // 3) Rol permitido (solo Administrador o Técnico)
                var rol = usuario.IdRolNavigation?.Descripcion?.Trim();
                if (rol != "Administrador" && rol != "Técnico")
                {
                    ModelState.AddModelError(string.Empty, "Su rol no tiene permisos para acceder al sistema.");
                    return View(model);
                }

                // 4) Construcción de Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
                    new Claim(ClaimTypes.Email, usuario.Correo ?? string.Empty),
                    new Claim(ClaimTypes.Role, rol ?? string.Empty),
                    new Claim("FotoUrl", usuario.UrlFoto ?? string.Empty)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                var authProps = new AuthenticationProperties
                {
                    IsPersistent = model.Recordarme,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProps
                );

                // 5) Redirección: si viene ReturnUrl válido, úsalo; si no, a Usuarios
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        
        // ============= LOGOUT =============

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Usuario");

            Response.Cookies.Delete(".AspNetCore.Cookies");

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        // ============= NAVBAR (nombre + avatar) =============

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> NavInfo()
        {
            var u = await _usuarioService.ObtenerPorId(UserId);
            var fotoUrl = string.IsNullOrWhiteSpace(u.UrlFoto)
                ? "/uploads/users/default.png"
                : u.UrlFoto;

            return Json(new
            {
                nombre = u.Nombre,
                foto = fotoUrl
            });
        }

        // ============= PERFIL =============
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var usuario = await _usuarioService.ObtenerPorId(UserId);
            var vm = _mapper.Map<VMPerfil>(usuario);

                string? url = vm.UrlFoto;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (!url.StartsWith("/")) url = "/" + url;

                    var physical = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                    if (!System.IO.File.Exists(physical))
                    url = "/img/user-default.png";
                }
                else
                {
                    url = "/img/user-default.png";
                }

            vm.UrlFoto = url;
            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Denegado()
        {
            return View(); // Vista simple: "No tiene permisos para acceder a este recurso"
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> GuardarPerfil(
         [FromForm] IFormFile? foto,
         [FromForm] string correo,
         [FromForm] string telefono)
         {
                var g = new GenericResponse<VMPerfil>();

                try
                {
                    var db = await _usuarioService.ObtenerPorId(UserId);
                    db.Correo = correo?.Trim() ?? string.Empty;
                    db.Telefono = telefono?.Trim() ?? string.Empty;

                    string nombreFoto = string.Empty;
                    Stream? stream = null;

                    if (foto is not null && foto.Length > 0)
                    {
                        nombreFoto = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
                        stream = foto.OpenReadStream();
                    }

                    var actualizado = await _usuarioService.Editar(db, stream, nombreFoto);
                    var vm = _mapper.Map<VMPerfil>(actualizado);

                    g.Estado = true;
                    g.Mensaje = "Perfil actualizado correctamente.";
                    g.Objeto = vm;

                    return StatusCode(StatusCodes.Status200OK, g);
                }
                catch (Exception ex)
                {
                    g.Estado = false;
                    g.Mensaje = ex.Message;
                    g.Objeto = null;
                    return StatusCode(StatusCodes.Status500InternalServerError, g);
                }
            }

            //Cambia la contraseña del usuario autenticado.
            [Authorize]
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CambiarClave(
                [FromForm] string actual,
                [FromForm] string nueva)
            {
                var g = new GenericResponse<string>();

                try
                {
                    if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(nueva))
                    {
                        g.Estado = false;
                        g.Mensaje = "Datos incompletos";
                        return BadRequest(g);
                    }

                    bool ok = await _usuarioService.CambiarClave(UserId, actual, nueva);

                    g.Estado = ok;
                    g.Mensaje = ok
                        ? "Contraseña actualizada"
                        : "No se pudo actualizar la contraseña";

                    return Ok(g);
                }
                catch (Exception ex)
                {
                    g.Estado = false;
                    g.Mensaje = ex.Message;
                    return StatusCode(StatusCodes.Status500InternalServerError, g);
                }
            }

            [Authorize]
            [HttpGet]
            public async Task<IActionResult> ObtenerAvatar()
            {
                try
                {
                    var usuario = await _usuarioService.ObtenerPorId(UserId);
                    return Json(new
                    {
                        nombre = usuario.Nombre,
                        foto = string.IsNullOrWhiteSpace(usuario.UrlFoto)
                            ? "/uploads/users/default.png"
                            : usuario.UrlFoto
                    });
                }
                catch
                {
                    return Json(new { nombre = "Usuario", foto = "/uploads/users/default.png" });
                }
            }
    
    }
}
