
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICorreoService _correoService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ILocalImageStorage _imageStore;

        public UsuarioService(
            IGenericRepository<Usuario> repositorio,
            IFirebaseService _,// Retenido para compatibilidad futura
            ICorreoService correoService,
            IUtilidadesService utilidadesService,
            ILocalImageStorage imageStore,
            IUsuarioRepository usuarioRepository)
        {
            _repositorio = repositorio;
            _usuarioRepository = usuarioRepository;
            _correoService = correoService;
            _utilidadesService = utilidadesService;
            _imageStore = imageStore;
        }

        public async Task<List<Usuario>> Lista()
        {
            return await _usuarioRepository.ListaConRolSinTracking();
        }

        public async Task<Usuario> Crear(
            Usuario entidad,
            Stream? foto = null,
            string NombreFoto = "",
            string urlPlantillaCorreo = ""
        )
        {
            // 1) Validaciones previas
            var existe = await _repositorio.ObtenerDatos(u => u.Correo == entidad.Correo);
            if (existe is not null)
                throw new TaskCanceledException("El correo ya existe");

            // 2) Generar y aplicar clave
            var clavePlano = _utilidadesService.GenerarClave();
            entidad.Clave = _utilidadesService.HashPassword(clavePlano);

            // 3) Crear usuario (sin imagen)
            var creado = await _repositorio.Crear(entidad);
            if (creado.IdUsuario == 0)
                throw new TaskCanceledException("No se pudo crear el usuario");

            // 4) Si viene imagen: guardar y actualizar campos
            if (foto is not null && !string.IsNullOrWhiteSpace(NombreFoto))
            {
                var savedFile = await _imageStore.SaveUserImageAsync(foto, NombreFoto);
                if (string.IsNullOrWhiteSpace(savedFile))
                    throw new TaskCanceledException("No se pudo guardar la imagen. Verifique formato y tamaño.");

                creado.NombreFoto = savedFile;
                creado.UrlFoto = _imageStore.GetUserImageUrl(savedFile);

                // Persistimos los cambios de imagen
                await _repositorio.Editar(creado);
            }

            // 5) Envío de correo (opcional)
            if (!string.IsNullOrWhiteSpace(urlPlantillaCorreo))
            {
                var url = urlPlantillaCorreo
                    .Replace("[Correo]", creado.Correo)
                    .Replace("[Clave]", clavePlano);

                using var http = new HttpClient();
                var html = await http.GetStringAsync(url);

                if (!await _correoService.EnviarCorreo(creado.Correo, "Cuenta creada", html))
                    throw new TaskCanceledException("No se pudo enviar el correo");
            }

            // 6) Devolver con su rol
            return await _usuarioRepository.ObtenerPorIdConRolSinTracking(creado.IdUsuario)
                ?? throw new TaskCanceledException("No se pudo obtener el usuario creado.");
        }

        public async Task<Usuario> Editar(
            Usuario entidad, 
            Stream? foto = null, 
            string NombreFoto = ""
        )
        {
            // 1) Correo duplicado (excluyendo el propio)
            var dup = await _repositorio.ObtenerDatos(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (dup is not null)
                throw new TaskCanceledException("El correo ya existe");

            // 2) Obtener usuario
            var usuario = await _usuarioRepository.ObtenerPorIdConRolTracking(entidad.IdUsuario)
                ?? throw new TaskCanceledException("El usuario no existe");

            // 3) Actualizar datos base
            usuario.Nombre = entidad.Nombre;
            usuario.Correo = entidad.Correo;
            usuario.Telefono = entidad.Telefono;
            usuario.IdRol = entidad.IdRol;
            usuario.EsActivo = entidad.EsActivo;

            // 4) Nueva foto ⇒ guardar y, si cambia, borrar la vieja
            if (foto is not null && !string.IsNullOrWhiteSpace(NombreFoto))
            {
                var oldFile = usuario.NombreFoto;

                // Delega validación/seguridad a LocalImageStore (extensión/tamaño)
                var savedFileName = await _imageStore.SaveUserImageAsync(foto, NombreFoto);
                if (string.IsNullOrWhiteSpace(savedFileName))
                    throw new TaskCanceledException("No se pudo guardar la imagen. Verifique formato y tamaño.");

                // Persistir nuevo nombre y URL pública consistente
                usuario.NombreFoto = savedFileName;
                usuario.UrlFoto = _imageStore.GetUserImageUrl(savedFileName);

                // Borrar antigua (best-effort)
                if (!string.IsNullOrWhiteSpace(oldFile) && oldFile != savedFileName)
                {
                    try { await _imageStore.DeleteUserImageAsync(oldFile); } catch { /* best-effort */ }
                }
            }

            // 5) Guardar y devolver con navegación de rol
            if (!await _repositorio.Editar(usuario))
                throw new TaskCanceledException("No se pudo editar el usuario");

            return await _usuarioRepository.ObtenerPorIdConRolSinTracking(usuario.IdUsuario)
                ?? throw new TaskCanceledException("No se pudo obtener el usuario actualizado.");
        }

        public async Task<bool> Eliminar(int idUsuario)
        {
            var usuario = await _repositorio.ObtenerDatos(u => u.IdUsuario == idUsuario)
            ?? throw new TaskCanceledException("El usuario no existe");

            var foto = usuario.NombreFoto;

            var ok = await _repositorio.Eliminar(usuario);
            if (!ok)
                throw new TaskCanceledException("No se pudo eliminar el usuario");

            if (!string.IsNullOrWhiteSpace(foto))
            {
                try { await _imageStore.DeleteUserImageAsync(foto); } catch { /* best-effort */ }
            }

            return true;
        }

        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            var usuario = await _usuarioRepository.ObtenerPorCorreoConRolTracking(correo);

            if (usuario is null || usuario.Clave == null || !_utilidadesService.VerifyPassword(clave, usuario.Clave))
                throw new TaskCanceledException("Credenciales incorrectas.");

            // Si aún es SHA-256, hacemos "rehash" a PBKDF2
            if (_utilidadesService.IsLegacySha256(usuario.Clave))
            {
                usuario.Clave = _utilidadesService.HashPassword(clave);
                await _repositorio.Editar(usuario);
            }

            return usuario;
        }

        public async Task<Usuario> ObtenerPorId(int idUsuario)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdConRolSinTracking(idUsuario);

            if (usuario is null) throw new TaskCanceledException("Usuario no existe!");

            return usuario;
        }

        public async Task<bool> CambiarClave(int idUsuario, string claveActual, string nuevaClave)
        {
            var usuario = await _repositorio.ObtenerDatos(u => u.IdUsuario == idUsuario)
                    ?? throw new TaskCanceledException("El usuario no existe!");

            if (usuario.Clave == null || !_utilidadesService.VerifyPassword(claveActual, usuario.Clave))
                throw new TaskCanceledException("La clave actual es incorrecta!");

            usuario.Clave = _utilidadesService.HashPassword(nuevaClave);

            if (!await _repositorio.Editar(usuario))
                throw new TaskCanceledException("No se pudo cambiar la clave!");

            return true;
        }

        public async Task<bool> RestablecerClave(string correo, string urlPlantillaCorreo)
        {
            var usuario = await _repositorio.ObtenerDatos(u => u.Correo == correo)
                ?? throw new TaskCanceledException("El usuario no existe.");

            var clavePlano = _utilidadesService.GenerarClave();
            usuario.Clave = _utilidadesService.HashPassword(clavePlano);

            using var http = new HttpClient();
            var html = await http.GetStringAsync(urlPlantillaCorreo.Replace("[Clave]", clavePlano));

            if (!await _correoService.EnviarCorreo(correo, "Contraseña restablecida", html))
                throw new TaskCanceledException("No se pudo enviar el correo.");

            return await _repositorio.Editar(usuario);
        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            var usuario = await _repositorio.ObtenerDatos(u => u.IdUsuario == entidad.IdUsuario)
                ?? throw new TaskCanceledException("El usuario no existe!");

            usuario.Correo = entidad.Correo;
            usuario.Telefono = entidad.Telefono;

            if (!await _repositorio.Editar(usuario))
                throw new TaskCanceledException("No se pudo guardar el perfil!");

            return true;
        }
    }
}
