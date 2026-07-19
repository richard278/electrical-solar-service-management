using EcoInstalacion.AppWeb.DTOs.Auth;
using EcoInstalacion.AppWeb.Services.Auth;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    [IgnoreAntiforgeryToken]
    public class AuthApiController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthApiController(IUsuarioService usuarioService, IJwtTokenService jwtTokenService)
        {
            _usuarioService = usuarioService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuario = await _usuarioService.ObtenerPorCredenciales(request.Correo, request.Clave);

                if (usuario == null)
                {
                    return Unauthorized(new { mensaje = "Credenciales incorrectas." });
                }

                if (usuario.EsActivo != true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "El usuario está inactivo. Contacte al administrador." });
                }

                var rolEntidad = usuario.IdRolNavigation;
                if (rolEntidad?.EsActivo != true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "El rol del usuario está inactivo. Contacte al administrador." });
                }

                var rol = rolEntidad.Descripcion?.Trim();
                if (!RolesSistema.CoincideConCatalogo(usuario.IdRol, rol))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Su rol no tiene permisos para acceder al sistema." });
                }

                var rolAutorizado = rol!;
                var (token, expiresAt, _) = _jwtTokenService.GenerateToken(usuario, rolAutorizado);

                var response = new AuthLoginResponseDto
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresAtUtc = expiresAt,
                    User = new AuthUserDto
                    {
                        Id = usuario.IdUsuario,
                        Nombre = usuario.Nombre ?? string.Empty,
                        Rol = rolAutorizado
                    }
                };

                return Ok(response);
            }
            catch (TaskCanceledException ex)
            {
                // ObtenerPorCredenciales lanza TaskCanceledException ante credenciales incorrectas.
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Loguear el error internamente de forma segura
                System.Diagnostics.Debug.WriteLine($"[AuthError] Error en login API: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al procesar el login." });
            }
        }
    }
}
