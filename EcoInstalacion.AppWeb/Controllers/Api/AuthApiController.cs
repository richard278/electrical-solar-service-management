using EcoInstalacion.AppWeb.DTOs.Auth;
using EcoInstalacion.AppWeb.Services.Auth;
using EcoInstalacion.BLL.Interfaces;
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

                if (usuario.EsActivo == false)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "El usuario está inactivo. Contacte al administrador." });
                }

                var rol = usuario.IdRolNavigation?.Descripcion?.Trim();
                if (rol != "Administrador" && rol != "Técnico")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Su rol no tiene permisos para acceder al sistema." });
                }

                var (token, expiresAt, expiresIn) = _jwtTokenService.GenerateToken(usuario, rol);

                var response = new AuthLoginResponseDto
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = expiresIn,
                    ExpiresAt = expiresAt,
                    User = new AuthUserDto
                    {
                        IdUsuario = usuario.IdUsuario,
                        Nombre = usuario.Nombre ?? string.Empty,
                        Correo = usuario.Correo ?? string.Empty,
                        Rol = rol,
                        FotoUrl = usuario.UrlFoto
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al procesar el login.", detalle = ex.Message });
            }
        }
    }
}
