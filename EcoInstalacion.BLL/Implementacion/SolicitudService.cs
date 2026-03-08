using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.EntityFrameworkCore;

namespace EcoInstalacion.BLL.Implementacion
{
    public class SolicitudService : ISolicitudService
    {
        private readonly IGenericRepository<SolicitudesInstalacion> _repositorio;

        public SolicitudService(IGenericRepository<SolicitudesInstalacion>repositorio)
        {
            _repositorio = repositorio;
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

            // Trae relaciones inmediatamente después de guardar (Cliente y Técnico)
            var completa = await _repositorio.Query(s => s.Id == creada.Id)
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .FirstOrDefaultAsync();

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

            // Devuelve con las relaciones cargadas
            var actualizada = await _repositorio.Query(s => s.Id == existente.Id)
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .Include(s => s.VisitasTecnicas)
                .FirstOrDefaultAsync();

            return actualizada ?? existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            var existente = await _repositorio.Query(s => s.Id == id)
                .Include(s => s.VisitasTecnicas)
                .FirstOrDefaultAsync();

            if (existente == null)
                return false;

            if (existente.VisitasTecnicas.Any())
                throw new TaskCanceledException("No se puede eliminar: existen visitas técnicas asociadas.");

            return await _repositorio.Eliminar(existente);
        }

        public async Task<List<SolicitudesInstalacion>> Lista()
        {
            var query = _repositorio.Query();
            return await query
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SolicitudesInstalacion?> ObtenerPorId(int id)
        {
            return await _repositorio.Query(s => s.Id == id)
                .Include(s => s.Cliente)
                .Include(s => s.TecnicoAsignado)
                .Include(s => s.VisitasTecnicas)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
