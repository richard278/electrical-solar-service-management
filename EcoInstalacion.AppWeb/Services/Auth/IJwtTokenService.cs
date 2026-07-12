using EcoInstalacion.Entity;
using System;

namespace EcoInstalacion.AppWeb.Services.Auth
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt, int ExpiresIn) GenerateToken(Usuario usuario, string rol);
    }
}
