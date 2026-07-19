using EcoInstalacion.AppWeb.Options;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Seguridad;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcoInstalacion.AppWeb.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public (string Token, DateTime ExpiresAt, int ExpiresIn) GenerateToken(Usuario usuario, string rol)
        {
            if (usuario.EsActivo != true ||
                usuario.IdRolNavigation?.EsActivo != true ||
                !RolesSistema.CoincideConCatalogo(usuario.IdRol, rol))
            {
                throw new InvalidOperationException("No se puede emitir un JWT para un usuario o rol no autorizado.");
            }

            if (string.IsNullOrWhiteSpace(_jwtOptions.SecretKey) ||
                _jwtOptions.SecretKey == "SET_THIS_VALUE_USING_USER_SECRETS_OR_ENVIRONMENT_VARIABLES")
            {
                throw new InvalidOperationException("La clave secreta de JWT (SecretKey) no está configurada.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException("La clave secreta de JWT (SecretKey) debe tener al menos 32 bytes para HMAC SHA-256.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutes = _jwtOptions.ExpirationMinutes;
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
            var expiresInSeconds = expirationMinutes * 60;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return (tokenString, expiresAt, expiresInSeconds);
        }
    }
}
