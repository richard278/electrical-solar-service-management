using EcoInstalacion.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoInstalacion.BLL.Interfaces
{
    public interface IRolService
    {
        Task<List<Rol>> Lista(string? filtro = null, bool? soloActivos = null);
        Task<Rol?> ObtenerPorId(int idRol);

        // El catálogo está cerrado; estos contratos se conservan por compatibilidad
        // y rechazan cualquier intento de mutación en la implementación.
        Task<Rol> Crear(Rol entidad);
        Task<Rol> Editar(Rol entidad);
        Task<bool> CambiarEstado(int idRol, bool esActivo);
        Task<bool> Eliminar(int idRol);
    }
}
