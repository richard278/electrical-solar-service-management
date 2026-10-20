using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Shared;

namespace EcoInstalacion.BLL.Implementacion
{
    public class ClienteService : IClienteService
    {
        private readonly IGenericRepository<Cliente> _repoCliente;
        private readonly IClienteRepository _repoClienteSpecific;

        public ClienteService(
            IGenericRepository<Cliente> repoCliente,
            IClienteRepository repoClienteSpecific)
        {
            _repoCliente = repoCliente;
            _repoClienteSpecific = repoClienteSpecific;
        }

        public async Task<Cliente> Crear(Cliente entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));

            // Unicidad por DUI
            if (!string.IsNullOrWhiteSpace(entidad.Dui))
            {
                var existeDui = await _repoCliente.ObtenerDatos(c => c.Dui == entidad.Dui);
                if (existeDui is not null) throw new TaskCanceledException("El DUI ya está registrado.");
            }
            // Unicidad por Correo (si viene)
            if (!string.IsNullOrWhiteSpace(entidad.Correo))
            {
                var existeCorreo = await _repoCliente.ObtenerDatos(c => c.Correo == entidad.Correo);
                if (existeCorreo is not null) throw new TaskCanceledException("El correo ya está registrado.");
            }

            var creado = await _repoCliente.Crear(entidad);
            if (creado is null || creado.Id <= 0) throw new TaskCanceledException("No se pudo crear el cliente.");
            return creado;
        }

        public async Task<Cliente> Editar(Cliente entidad)
        {
            if (entidad is null) throw new ArgumentNullException(nameof(entidad));
            var existente = await _repoCliente.ObtenerDatos(c => c.Id == entidad.Id);
            if (existente is null) throw new KeyNotFoundException("El cliente no existe.");

            if (!string.Equals(existente.Dui, entidad.Dui, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(entidad.Dui))
            {
                var dupDui = await _repoCliente.ObtenerDatos(c => c.Dui == entidad.Dui);
                if (dupDui is not null) throw new TaskCanceledException("Otro cliente ya tiene ese DUI.");
            }
            if (!string.Equals(existente.Correo, entidad.Correo, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(entidad.Correo))
            {
                var dupCorreo = await _repoCliente.ObtenerDatos(c => c.Correo == entidad.Correo);
                if (dupCorreo is not null) throw new TaskCanceledException("Otro cliente ya tiene ese correo.");
            }

            existente.NombreCompleto = entidad.NombreCompleto;
            existente.Dui = entidad.Dui;
            existente.Telefono = entidad.Telefono;
            existente.Direccion = entidad.Direccion;
            existente.Correo = entidad.Correo;
            existente.EsActivo = entidad.EsActivo;

            var ok = await _repoCliente.Editar(existente);
            if (!ok) throw new TaskCanceledException("No se pudo editar el cliente.");
            return existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            // Bloquear si tiene solicitudes
            var tieneSolicitudes = await _repoClienteSpecific.TieneSolicitudes(id);
            if (tieneSolicitudes)
                throw new TaskCanceledException("No se puede eliminar: el cliente tiene solicitudes registradas.");

            var existente = await _repoCliente.ObtenerDatos(c => c.Id == id);
            if (existente is null) return false;

            return await _repoCliente.Eliminar(existente);
        }


        //revisar este servicio
        public async Task<List<Cliente>> Lista()
        {
            return await _repoClienteSpecific.ListaActivosSinTracking();
        }

        public async Task<List<Cliente>> ListaPorEstado(bool? esActivo)
        {
            return await _repoClienteSpecific.ListaPorEstadoSinTracking(esActivo);
        }

        public async Task<Cliente?> ObtenerPorId(int id)
        {
            return await _repoClienteSpecific.ObtenerPorIdSinTracking(id);
        }

        public async Task<PagedResult<Cliente>> ListaPaginada(
            string? q = null,
            bool? esActivo = null,
            int page = 1,
            int pageSize = 25)
        {
            return await _repoClienteSpecific.ListaPaginadaSinTracking(
                q,
                esActivo,
                page,
                pageSize);
        }
    }
}
