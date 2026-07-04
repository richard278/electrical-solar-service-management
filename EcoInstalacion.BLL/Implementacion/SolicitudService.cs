using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class SolicitudService : ISolicitudService
    {
        private readonly IGenericRepository<SolicitudesInstalacion> _repositorio;
        private readonly ISolicitudRepository _solicitudRepository;

        public SolicitudService(
            IGenericRepository<SolicitudesInstalacion> repositorio,
            ISolicitudRepository solicitudRepository)
        {
            _repositorio = repositorio;
            _solicitudRepository = solicitudRepository;
        }

        public async Task<SolicitudesInstalacion> Crear(SolicitudesInstalacion entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.FechaSolicitud == default)
                entidad.FechaSolicitud = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(entidad.Estado))
                entidad.Estado = "Pendiente";

            // Validación de duplicado (Cliente + Descripción abierta)
            if (!string.IsNullOrWhiteSpace(entidad.Descripcion))
            {
                var duplicada = await _repositorio.ObtenerDatos(s =>
                    s.ClienteId == entidad.ClienteId &&
                    s.Descripcion == entidad.Descripcion &&
                    s.Estado != "Cerrada");
                if (duplicada != null)
                    throw new TaskCanceledException("Ya existe una solicitud abierta con esa descripción para este cliente.");
            }

            var creada = await _repositorio.Crear(entidad);
            if (creada == null || creada.Id <= 0)
                throw new TaskCanceledException("No se pudo crear la solicitud.");

            // Recarga con relaciones desde DAL
            var completa = await _solicitudRepository.ObtenerConRelaciones(creada.Id);
            return completa ?? creada;
        }

        public async Task<SolicitudesInstalacion> Editar(SolicitudesInstalacion entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            var existente = await _repositorio.ObtenerDatos(s => s.Id == entidad.Id);
            if (existente == null)
                throw new KeyNotFoundException("La solicitud no existe.");

            existente.FechaSolicitud = entidad.FechaSolicitud == default ? existente.FechaSolicitud : entidad.FechaSolicitud;
            existente.ClienteId = entidad.ClienteId;
            existente.TipoInstalacion = entidad.TipoInstalacion;
            existente.Descripcion = entidad.Descripcion;
            existente.Estado = entidad.Estado;
            existente.TecnicoAsignadoId = entidad.TecnicoAsignadoId;

            var ok = await _repositorio.Editar(existente);
            if (!ok)
                throw new TaskCanceledException("No se pudo editar la solicitud.");

            // Recarga con relaciones desde DAL
            var actualizada = await _solicitudRepository.ObtenerConRelaciones(existente.Id);
            return actualizada ?? existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            // Carga solicitud con visitas desde DAL (con tracking para permitir Remove)
            var existente = await _solicitudRepository.ObtenerConVisitas(id);

            if (existente == null)
                return false;

            // Regla de negocio: no eliminar si existen visitas técnicas asociadas
            if (existente.VisitasTecnicas != null && existente.VisitasTecnicas.Any())
                throw new TaskCanceledException("No se puede eliminar: existen visitas técnicas asociadas.");

            return await _repositorio.Eliminar(existente);
        }

        public async Task<List<SolicitudesInstalacion>> Lista()
        {
            return await _solicitudRepository.ListaConRelaciones();
        }

        public async Task<SolicitudesInstalacion?> ObtenerPorId(int id)
        {
            return await _solicitudRepository.ObtenerConRelaciones(id);
        }
    }
}
