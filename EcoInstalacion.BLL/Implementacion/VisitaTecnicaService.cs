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

            // Validar solicitud existente
            var solicitud = await _repoSolicitud.ObtenerDatos(s => s.Id == entidad.SolicitudId);
            if (solicitud is null) throw new TaskCanceledException("La solicitud asociada no existe.");

            if (entidad.FechaVisita == default) entidad.FechaVisita = DateTime.UtcNow;

            var creada = await _repoVisita.Crear(entidad);
            if (creada is null || creada.Id <= 0) throw new TaskCanceledException("No se pudo crear la visita técnica.");
            return creada;
        }

        public async Task<VisitasTecnica> Editar(VisitasTecnica entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));

            var existente = await _repoVisita.ObtenerDatos(v => v.Id == entidad.Id);
            if (existente is null) throw new KeyNotFoundException("La visita técnica no existe.");

            if (existente.SolicitudId != entidad.SolicitudId)
            {
                var solicitud = await _repoSolicitud.ObtenerDatos(s => s.Id == entidad.SolicitudId);
                if (solicitud is null) throw new TaskCanceledException("La nueva solicitud no existe.");
                existente.SolicitudId = entidad.SolicitudId;
            }

            existente.FechaVisita = entidad.FechaVisita == default ? existente.FechaVisita : entidad.FechaVisita; // <-- unificado
            existente.Observaciones = entidad.Observaciones;
            existente.Aprobado = entidad.Aprobado;

            var ok = await _repoVisita.Editar(existente);
            if (!ok) throw new TaskCanceledException("No se pudo editar la visita técnica.");

            // Devolver con relaciones
            var actualizado = await ObtenerPorId(existente.Id);
            return actualizado ?? existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            var existente = await _repoVisita.ObtenerDatos(v => v.Id == id);
            if (existente is null) return false;

            return await _repoVisita.Eliminar(existente);
        }

        public async Task<List<VisitasTecnica>> Lista()
        {
            return await _visitaTecnicaRepository.ListaConSolicitudCliente();
        }

        public async Task<List<VisitasTecnica>> ListaPorSolicitud(int solicitudId)
        {
            return await _visitaTecnicaRepository.ListaPorSolicitudConSolicitudCliente(solicitudId);
        }

        public async Task<VisitasTecnica?> ObtenerPorId(int id)
        {
            return await _visitaTecnicaRepository.ObtenerPorIdConSolicitudCliente(id);
        }
    }
}
