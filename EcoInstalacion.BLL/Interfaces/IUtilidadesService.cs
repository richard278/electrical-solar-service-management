using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IUtilidadesService
    {
        string GenerarClave();
        // === Legacy (aún usado al migrar) ===
        string ConvertirSHA256(string texto);
        // === Hashing moderno (PBKDF2) ===
        string HashPassword(string plain);// devuelve "PBKDF2$<iter>$<saltB64>$<hashB64>"
        bool VerifyPassword(string plain, string storedHash);// soporta PBKDF2 y fallback SHA256
        bool IsLegacySha256(string storedHash);// true si es un hex puro de 64 chars
    }
}
