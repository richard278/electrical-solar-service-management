using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.Interfaces
{
    public interface ISolicitudRepository
    {
        /// <summary>
        /// Lista todas las solicitudes con Cliente y TecnicoAsignado cargados.
        /// Ordenadas por FechaSolicitud descendente, luego por Id descendente. Sin tracking.
        /// </summary>
        Task<List<SolicitudesInstalacion>> ListaConRelaciones();

        /// <summary>
        /// Obtiene una solicitud por Id con Cliente, TecnicoAsignado y VisitasTecnicas cargados.
        /// Sin tracking. Usado en ObtenerPorId y recargas post Crear/Editar.
        /// </summary>
        Task<SolicitudesInstalacion?> ObtenerConRelaciones(int id);

        /// <summary>
        /// Obtiene una solicitud por Id con VisitasTecnicas cargadas.
        /// Con tracking (necesario para que GenericRepository.Eliminar pueda remover la entidad).
        /// Usado exclusivamente para validar integridad referencial antes de eliminar.
        /// </summary>
        Task<SolicitudesInstalacion?> ObtenerConVisitas(int id);

        /// <summary>
        /// Obtiene una lista paginada de solicitudes con Cliente y TecnicoAsignado cargados, filtrada según la consulta DTO.
        /// Sin tracking.
        /// </summary>
        Task<EcoInstalacion.Entity.Pagination.PagedResultDto<SolicitudesInstalacion>> ListaPaginadaSinTracking(EcoInstalacion.Entity.Pagination.SolicitudQueryDto query);
    }
}
