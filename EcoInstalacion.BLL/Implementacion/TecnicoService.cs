using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class TecnicoService : ITecnicoService
    {
        private readonly IGenericRepository<Tecnico> _repoTecnico;
        private readonly ITecnicoRepository _tecnicoRepository;

        public TecnicoService(
            IGenericRepository<Tecnico> repoTecnico,
            ITecnicoRepository tecnicoRepository)
        {
            _repoTecnico = repoTecnico;
            _tecnicoRepository = tecnicoRepository;
        }

        public async Task<Tecnico> Crear(Tecnico entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));

            if (!string.IsNullOrWhiteSpace(entidad.Correo))
            {
                var dupMail = await _repoTecnico.ObtenerDatos(t => t.Correo == entidad.Correo);
                if (dupMail is not null) throw new TaskCanceledException("El correo ya está registrado para otro técnico.");
            }

            var creado = await _repoTecnico.Crear(entidad);
            if (creado is null || creado.Id <= 0) throw new TaskCanceledException("No se pudo crear el técnico.");
            return creado;
        }

        public async Task<Tecnico> Editar(Tecnico entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));
            var existente = await _repoTecnico.ObtenerDatos(t => t.Id == entidad.Id);
            if (existente is null) throw new KeyNotFoundException("El técnico no existe.");

            if (!string.Equals(existente.Correo, entidad.Correo, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(entidad.Correo))
            {
                var dupMail = await _repoTecnico.ObtenerDatos(t => t.Correo == entidad.Correo);
                if (dupMail is not null) throw new TaskCanceledException("Otro técnico ya tiene ese correo.");
            }

            existente.Nombre = entidad.Nombre;
            existente.Especialidad = entidad.Especialidad;
            existente.Telefono = entidad.Telefono;
            existente.Correo = entidad.Correo;

            var ok = await _repoTecnico.Editar(existente);
            if (!ok) throw new TaskCanceledException("No se pudo editar el técnico.");
            return existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            // Bloquear si está asignado en alguna solicitud
            var asignado = await _tecnicoRepository.EstaAsignadoASolicitudes(id);
            if (asignado)
                throw new TaskCanceledException("No se puede eliminar: el técnico está asignado a solicitudes.");

            var existente = await _repoTecnico.ObtenerDatos(t => t.Id == id);
            if (existente is null) return false;

            return await _repoTecnico.Eliminar(existente);
        }

        public async Task<List<Tecnico>> Lista()
        {
            return await _tecnicoRepository.ListaSinTracking();
        }

        public async Task<Tecnico?> ObtenerPorId(int id)
        {
            return await _tecnicoRepository.ObtenerPorIdSinTracking(id);
        }
    }
}
