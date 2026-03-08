using EcoInstalacion.BLL.Interfaces;
using System.Security.Cryptography;
using EcoInstalacion.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoInstalacion.BLL.Implementacion
{
    public class UtilidadesService : IUtilidadesService
    {
        private const int SaltSize = 16;// 128-bit
        private const int KeySize = 32; // 256-bit
        private const int Iterations = 100_000;// costo recomendado

        public string GenerarClave() => Guid.NewGuid().ToString("N")[..6];
        
        // ---- Legacy SHA-256 (hex) ----
        public string ConvertirSHA256(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public bool IsLegacySha256(string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;
            // 64 hex chars → sha256 clásico
            return storedHash.Length == 64 && storedHash.All(c =>
                (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
        }

        // ---- PBKDF2 moderno ----
        public string HashPassword(string plain)
        {
            // sal aleatoria
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password: plain,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256
            );
            var key = pbkdf2.GetBytes(KeySize);

            return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public bool VerifyPassword(string plain, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            // Formato PBKDF2$iter$salt$hash
            if (storedHash.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[1], out var iters)) return false;
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password: plain,
                    salt: salt,
                    iterations: iters,
                    hashAlgorithm: HashAlgorithmName.SHA256
                );
                var actual = pbkdf2.GetBytes(expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            // Fallback: SHA-256 legado
            if (IsLegacySha256(storedHash))
            {
                var hex = ConvertirSHA256(plain);
                // comparación timing-safe
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(hex),
                    Encoding.ASCII.GetBytes(storedHash)
                );
            }

            return false;
        }
        /*
        Ventajas: no cambias la base; sigue usando la columna clave. 
        Los nuevos valores quedan como
        PBKDF2$100000$<saltB64>$<hashB64>. 
        Los anteriores siguen en HEX y se migran al vuelo.
        */
    }
}
