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

        Task<Rol> Crear(Rol entidad);
        Task<Rol> Editar(Rol entidad);
        Task<bool> CambiarEstado(int idRol, bool esActivo);
        // Elimina físicamente el rol sólo si no tiene usuarios asignados.
        Task<bool> Eliminar(int idRol);
    }
}
