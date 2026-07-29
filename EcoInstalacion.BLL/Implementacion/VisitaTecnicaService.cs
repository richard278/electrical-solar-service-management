using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class VisitaTecnicaService : IVisitaTecnicaService
    {
        private readonly IGenericRepository<VisitasTecnica> _repoVisita;
        private readonly IGenericRepository<SolicitudesInstalacion> _repoSolicitud;
        private readonly IVisitaTecnicaRepository _visitaTecnicaRepository;

        public VisitaTecnicaService(
            IGenericRepository<VisitasTecnica> repoVisita,
            IGenericRepository<SolicitudesInstalacion> repoSolicitud,
            IVisitaTecnicaRepository visitaTecnicaRepository)
        {
            _repoVisita = repoVisita;
            _repoSolicitud = repoSolicitud;
            _visitaTecnicaRepository = visitaTecnicaRepository;
        }

        public async Task<VisitasTecnica> Crear(VisitasTecnica entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));

            NormalizarYValidarCampos(entidad);

            var solicitud = await _repoSolicitud.ObtenerDatos(s => s.Id == entidad.SolicitudId);
            ValidarSolicitudParaNuevaVisita(solicitud);

            entidad.Aprobado = entidad.Aprobado == true;

            var creada = await _repoVisita.Crear(entidad);
            if (creada is null || creada.Id <= 0) throw new TaskCanceledException("No se pudo crear la visita técnica.");

            // Si nació aprobada, el trigger avanza la solicitud dentro de la
            // misma operación de inserción. La recarga devuelve ese estado.
            var completa = await ObtenerPorId(creada.Id);
            return completa ?? creada;
        }

        public async Task<VisitasTecnica> Editar(VisitasTecnica entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));

            var existente = await _repoVisita.ObtenerDatos(v => v.Id == entidad.Id);
            if (existente is null) throw new KeyNotFoundException("La visita técnica no existe.");

            if (existente.SolicitudId != entidad.SolicitudId)
                throw new InvalidOperationException("No se puede cambiar la solicitud asociada a una visita existente.");

            NormalizarYValidarCampos(entidad);

            var estabaAprobada = existente.Aprobado == true;
            var aprobar = entidad.Aprobado == true;

            if (estabaAprobada && !aprobar)
                throw new InvalidOperationException("Una visita aprobada no puede volver al estado pendiente.");

            var avanzarSolicitud = !estabaAprobada && aprobar;
            if (avanzarSolicitud)
            {
                var solicitud = await _repoSolicitud.ObtenerDatos(s => s.Id == existente.SolicitudId);
                ValidarSolicitudParaAprobacion(solicitud);
            }

            existente.FechaVisita = entidad.FechaVisita;
            existente.Observaciones = entidad.Observaciones;
            existente.Aprobado = aprobar;

            var ok = await _visitaTecnicaRepository.EditarConTransicionAprobacion(
                existente,
                avanzarSolicitud);
            if (!ok) throw new TaskCanceledException("No se pudo editar la visita técnica.");

            // Devolver con relaciones
            var actualizado = await ObtenerPorId(existente.Id);
            return actualizado ?? existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            var existente = await _repoVisita.ObtenerDatos(v => v.Id == id);
            if (existente is null) return false;

            if (existente.Aprobado == true)
                throw new InvalidOperationException("No se puede eliminar una visita aprobada.");

            return await _repoVisita.Eliminar(existente);
        }

        public async Task<List<VisitasTecnica>> Lista()
        {
            return await _visitaTecnicaRepository.ListaConSolicitudCliente();
        }

        public async Task<List<VisitasTecnica>> ListaPorSolicitud(int solicitudId)
        {
            if (solicitudId <= 0)
                throw new ArgumentException("La solicitud indicada no es válida.");

            return await _visitaTecnicaRepository.ListaPorSolicitudConSolicitudCliente(solicitudId);
        }

        public async Task<VisitasTecnica?> ObtenerPorId(int id)
        {
            return await _visitaTecnicaRepository.ObtenerPorIdConSolicitudCliente(id);
        }

        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<VisitasTecnica>> ListaPaginada(
            EcoInstalacion.Entity.Pagination.VisitaTecnicaQueryDto query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            return await _visitaTecnicaRepository.ListaPaginadaSinTracking(query);
        }

        private static void NormalizarYValidarCampos(VisitasTecnica entidad)
        {
            if (entidad.SolicitudId <= 0)
                throw new ArgumentException("Debe seleccionar una solicitud válida.");

            if (entidad.FechaVisita == default)
                throw new ArgumentException("La fecha de visita es obligatoria.");

            // SQL Server datetime no admite fechas anteriores a 1753.
            if (entidad.FechaVisita < new DateTime(1753, 1, 1))
                throw new ArgumentException("La fecha de visita no es válida.");

            var observaciones = entidad.Observaciones?.Trim();
            if (observaciones?.Length > 500)
                throw new ArgumentException("Las observaciones no deben exceder 500 caracteres.");

            entidad.Observaciones = string.IsNullOrWhiteSpace(observaciones)
                ? null
                : observaciones;
        }

        private static void ValidarSolicitudParaNuevaVisita(SolicitudesInstalacion? solicitud)
        {
            if (solicitud is null)
                throw new ArgumentException("La solicitud asociada no existe.");

            var estado = EstadosSolicitudSistema.NormalizarRequerido(solicitud.Estado);
            if (EstadosSolicitudSistema.EsTerminal(estado))
                throw new InvalidOperationException("No se pueden registrar visitas en una solicitud cerrada o rechazada.");
        }

        private static void ValidarSolicitudParaAprobacion(SolicitudesInstalacion? solicitud)
        {
            if (solicitud is null)
                throw new ArgumentException("La solicitud asociada no existe.");

            var estado = EstadosSolicitudSistema.NormalizarRequerido(solicitud.Estado);
            if (EstadosSolicitudSistema.EsTerminal(estado))
                throw new InvalidOperationException("No se puede aprobar una visita de una solicitud cerrada o rechazada.");
        }
    }
}
