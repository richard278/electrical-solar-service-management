using System;

namespace EcoInstalacion.AppWeb.DTOs.Auth
{
    public class AuthLoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAtUtc { get; set; }
        public AuthUserDto User { get; set; } = null!;
    }
}
