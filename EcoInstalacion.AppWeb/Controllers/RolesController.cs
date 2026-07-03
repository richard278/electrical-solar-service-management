using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : BaseController
    {
        private readonly IRolService _rolService;
        private readonly IMapper _mapper;

        public RolesController(IRolService rolService, IMapper mapper)
        {
            _rolService = rolService;
            _mapper = mapper;
        }

        // GET: /Roles
        [HttpGet]
        public override IActionResult Index() => View();

        // GET: /Roles/Lista
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var data = await _rolService.Lista();
            var vm = _mapper.Map<List<VMRol>>(data);

            // devolvemos JSON directo (lo que ya viste en el navegador)
            return Json(vm);
        }

        // GET: /Roles/ObtenerPorId/3
        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var r = new GenericResponse<VMRol>();
            try
            {
                var db = await _rolService.ObtenerPorId(id);
                if (db is null)
                {
                    r.Estado = false;
                    r.Mensaje = "Rol no encontrado";
                }
                else
                {
                    r.Estado = true;
                    r.Objeto = _mapper.Map<VMRol>(db);
                }
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, r);
        }

        // POST: /Roles/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromBody] VMRol vm)
        {
            var r = new GenericResponse<VMRol>();

            try
            {
                if (!ModelState.IsValid)
                {
                    r.Estado = false;
                    r.Mensaje = string.Join(" | ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(r);
                }

                var creado = await _rolService.Crear(_mapper.Map<Rol>(vm));
                r.Estado = true;
                r.Objeto = _mapper.Map<VMRol>(creado);
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, r);
        }

        // PUT: /Roles/Editar
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromBody] VMRol vm)
        {
            var r = new GenericResponse<VMRol>();

            try
            {
                var editado = await _rolService.Editar(_mapper.Map<Rol>(vm));
                r.Estado = true;
                r.Objeto = _mapper.Map<VMRol>(editado);
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, r);
        }

        // DELETE: /Roles/Eliminar?id=3
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var r = new GenericResponse<string>();

            try
            {
                r.Estado = await _rolService.Eliminar(id);
                r.Mensaje = r.Estado
                    ? "Rol eliminado."
                    : "No se pudo eliminar el rol.";
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, r);
        }

    }
}
