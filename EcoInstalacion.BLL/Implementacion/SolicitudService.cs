using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class SolicitudService : ISolicitudService
    {
        private readonly IGenericRepository<SolicitudesInstalacion> _repositorio;
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<Tecnico> _tecnicoRepository;

        public SolicitudService(
            IGenericRepository<SolicitudesInstalacion> repositorio,
            ISolicitudRepository solicitudRepository,
            IGenericRepository<Cliente> clienteRepository,
            IGenericRepository<Tecnico> tecnicoRepository)
        {
            _repositorio = repositorio;
            _solicitudRepository = solicitudRepository;
            _clienteRepository = clienteRepository;
            _tecnicoRepository = tecnicoRepository;
        }

        public async Task<SolicitudesInstalacion> Crear(SolicitudesInstalacion entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            NormalizarYValidarCampos(entidad);
            await ValidarClienteParaCrear(entidad.ClienteId);
            await ValidarTecnico(entidad.TecnicoAsignadoId);

            if (entidad.FechaSolicitud == default)
                entidad.FechaSolicitud = DateTime.UtcNow;

            // Toda solicitud nace pendiente. Las visitas aprobadas avanzan el
            // estado a "En proceso" mediante el trigger de SQL Server.
            entidad.Estado = EstadosSolicitudSistema.Pendiente;

            await ValidarDuplicado(entidad.ClienteId, entidad.Descripcion);

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

            NormalizarYValidarCampos(entidad);
            entidad.Estado = EstadosSolicitudSistema.NormalizarRequerido(entidad.Estado);

            await ValidarClienteParaEditar(entidad.ClienteId, existente.ClienteId);
            await ValidarTecnico(entidad.TecnicoAsignadoId);
            await ValidarDuplicado(
                entidad.ClienteId,
                entidad.Descripcion,
                existente.Id);

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

        public async Task<EcoInstalacion.Entity.Pagination.PagedResultDto<SolicitudesInstalacion>> ListaPaginada(
            EcoInstalacion.Entity.Pagination.SolicitudQueryDto query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            return await _solicitudRepository.ListaPaginadaSinTracking(query);
        }

        private static void NormalizarYValidarCampos(SolicitudesInstalacion entidad)
        {
            if (entidad.ClienteId <= 0)
                throw new ArgumentException("Debe seleccionar un cliente válido.");

            var tipoInstalacion = entidad.TipoInstalacion?.Trim();
            if (string.IsNullOrWhiteSpace(tipoInstalacion) || tipoInstalacion.Length is < 3 or > 50)
            {
                throw new ArgumentException(
                    "El tipo de instalación debe tener entre 3 y 50 caracteres.");
            }

            var descripcion = entidad.Descripcion?.Trim();
            if (descripcion?.Length > 500)
            {
                throw new ArgumentException(
                    "La descripción no debe exceder 500 caracteres.");
            }

            if (entidad.TecnicoAsignadoId.HasValue && entidad.TecnicoAsignadoId.Value <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un técnico válido.");
            }

            entidad.TipoInstalacion = tipoInstalacion;
            entidad.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion;
        }

        private async Task ValidarClienteParaCrear(int clienteId)
        {
            var cliente = await _clienteRepository.ObtenerDatos(c => c.Id == clienteId);
            if (cliente == null)
                throw new ArgumentException("El cliente seleccionado no existe.");

            if (!cliente.EsActivo)
                throw new InvalidOperationException("El cliente seleccionado está inactivo.");
        }

        private async Task ValidarClienteParaEditar(int clienteId, int clienteIdActual)
        {
            var cliente = await _clienteRepository.ObtenerDatos(c => c.Id == clienteId);
            if (cliente == null)
                throw new ArgumentException("El cliente seleccionado no existe.");

            if (clienteId != clienteIdActual && !cliente.EsActivo)
            {
                throw new InvalidOperationException(
                    "No se puede reasignar la solicitud a un cliente inactivo.");
            }
        }

        private async Task ValidarTecnico(int? tecnicoId)
        {
            if (!tecnicoId.HasValue)
                return;

            var tecnico = await _tecnicoRepository.ObtenerDatos(t => t.Id == tecnicoId.Value);
            if (tecnico == null)
                throw new ArgumentException("El técnico seleccionado no existe.");
        }

        private async Task ValidarDuplicado(
            int clienteId,
            string? descripcion,
            int? solicitudIdIgnorada = null)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return;

            var duplicada = await _repositorio.ObtenerDatos(s =>
                s.ClienteId == clienteId &&
                s.Descripcion == descripcion &&
                (!solicitudIdIgnorada.HasValue || s.Id != solicitudIdIgnorada.Value) &&
                s.Estado != EstadosSolicitudSistema.Cerrada &&
                s.Estado != EstadosSolicitudSistema.Rechazada);

            if (duplicada != null)
            {
                throw new TaskCanceledException(
                    "Ya existe una solicitud abierta con esa descripción para este cliente.");
            }
        }
    }
}
