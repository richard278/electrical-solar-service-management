using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IFirebaseService
    {
        Task<string> SubirStorage(Stream archivo, string carpetaDestino, string nombreArchivo);

        Task<bool> EliminarStorage(string rutaArchivo, string nombreArchivo);
    }
}
