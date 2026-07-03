using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Controllers
{
    // Solo Administrador puede entrar a este módulo
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IMapper _mapper;

        public UsuariosController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IMapper mapper)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _mapper = mapper;
        }

        [HttpGet]
        public override IActionResult Index() => View();

        #region Listas

        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            var roles = await _rolService.Lista();
            var vm = _mapper.Map<List<VMRol>>(roles);
            return StatusCode(StatusCodes.Status200OK, vm);
        }

        [HttpGet]
        public async Task<IActionResult> ListaUsuarios()
        {
            var data = await _usuarioService.Lista();
            var vm = _mapper.Map<List<VMUsuario>>(data);
            return StatusCode(StatusCodes.Status200OK, vm);
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(int id)
        {
            var usuario = await _usuarioService.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            var vm = _mapper.Map<VMUsuario>(usuario);
            return StatusCode(StatusCodes.Status200OK, vm);
        }

        #endregion

        #region Crear / Editar / Eliminar

        // =======================
        // CREAR USUARIO
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [FromForm] IFormFile? foto,
            [FromForm] string modelo)
        {
            var g = new GenericResponse<VMUsuario>();

            try
            {
                // 1) Deserializar el JSON que viene en "modelo"
                var vm = JsonConvert.DeserializeObject<VMUsuario>(modelo) ?? new VMUsuario();

                // 2) Limpiar y validar con DataAnnotations
                ModelState.Clear();
                if (!TryValidateModel(vm))
                {
                    g.Estado = false;
                    g.Mensaje = string.Join(" | ",
                        ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                    // Devolvemos 200 para que el front solo revise r.estado
                    return StatusCode(StatusCodes.Status200OK, g);
                }

                // 3) Manejo de foto (opcional)
                string nombreFoto = string.Empty;
                Stream? fotoStream = null;

                if (foto != null && foto.Length > 0)
                {
                    nombreFoto = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
                    fotoStream = foto.OpenReadStream();
                }

                // 4) (Opcional) plantilla de correo
                string urlPlantillaCorreo = string.Empty;

                // 5) Crear en la capa de negocio
                var creado = await _usuarioService.Crear(
                    _mapper.Map<Usuario>(vm),
                    fotoStream,
                    nombreFoto,
                    urlPlantillaCorreo);

                g.Estado = true;
                g.Objeto = _mapper.Map<VMUsuario>(creado);

                return StatusCode(StatusCodes.Status200OK, g);
            }
            catch (Exception ex)
            {
                g.Estado = false;
                g.Mensaje = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, g);
            }
        }

        // =======================
        // EDITAR USUARIO
        // =======================
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            [FromForm] IFormFile? foto,
            [FromForm] string modelo)
        {
            var g = new GenericResponse<VMUsuario>();

            try
            {
                var vm = JsonConvert.DeserializeObject<VMUsuario>(modelo) ?? new VMUsuario();

                // 1) Validar igual que en Crear
                ModelState.Clear();
                if (!TryValidateModel(vm))
                {
                    g.Estado = false;
                    g.Mensaje = string.Join(" | ",
                        ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                    return StatusCode(StatusCodes.Status200OK, g);
                }

                // 2) Foto opcional
                string nombreFoto = string.Empty;
                Stream? fotoStream = null;

                if (foto != null && foto.Length > 0)
                {
                    nombreFoto = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
                    fotoStream = foto.OpenReadStream();
                }

                // 3) Editar en capa de negocio
                var editado = await _usuarioService.Editar(
                    _mapper.Map<Usuario>(vm),
                    fotoStream,
                    nombreFoto);

                g.Estado = true;
                g.Objeto = _mapper.Map<VMUsuario>(editado);

                return StatusCode(StatusCodes.Status200OK, g);
            }
            catch (Exception ex)
            {
                g.Estado = false;
                g.Mensaje = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, g);
            }
        }

        // =======================
        // ELIMINAR USUARIO
        // =======================
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idUsuario)
        {
            var g = new GenericResponse<string>();

            try
            {
                bool ok = await _usuarioService.Eliminar(idUsuario);
                g.Estado = ok;
                g.Mensaje = ok
                    ? "Usuario eliminado correctamente."
                    : "No se pudo eliminar el usuario.";

                return StatusCode(StatusCodes.Status200OK, g);
            }
            catch (Exception ex)
            {
                g.Estado = false;
                g.Mensaje = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, g);
            }
        }

        #endregion
    }
}
