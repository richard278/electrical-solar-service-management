using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcoInstalacion.AppWeb.Controllers
{
    // Solo Administrador y Técnico pueden ver/gestionar solicitudes
    [Authorize(Roles = "Administrador,Técnico")]
    public class SolicitudesController : BaseController
    {
        private readonly ISolicitudService _solicitudService;
        private readonly IClienteService _clienteService;
        private readonly ITecnicoService _tecnicoService;
        
        public SolicitudesController(
            ISolicitudService solicitudService,
            IClienteService clienteService,
            ITecnicoService tecnicoService)
        {
            _solicitudService = solicitudService;
            _clienteService = clienteService;
            _tecnicoService = tecnicoService;
        }

        public IActionResult Index() => View();
     
        // --- util privado para juntar errores del ModelState ---
        private static string JoinModelErrors(ModelStateDictionary modelState) =>
            string.Join(" | ", modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        // --- data para combos (clientes / técnicos / estados) ---
        [HttpGet]
        public async Task<IActionResult> DataCombos()
        {
            var r = new GenericResponse<object>();
            try
            {
                var clientes = (await _clienteService.Lista())
                                .Select(c => new { c.Id, c.NombreCompleto })
                                .OrderBy(x => x.NombreCompleto)
                                .ToList();

                var tecnicos = (await _tecnicoService.Lista())
                                .Select(t => new { t.Id, t.Nombre })
                                .OrderBy(x => x.Nombre)
                                .ToList();

                r.Estado = true;
                r.Objeto = new
                {
                    clientes,
                    tecnicos,
                    estados = EstadosSolicitud.Todos
                };
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var r = new GenericResponse<VMSolicitudesInstalacion>();
            try
            {
                var lista = await _solicitudService.Lista();
                r.Estado = true;
                r.Lista = lista.Select(s => s.ToVM()).ToList();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // 🔐 Crear (POST JSON + AntiForgery)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] VMSolicitudesInstalacion vm)
        {
            var r = new GenericResponse<VMSolicitudesInstalacion>();
            try
            {
                if (!ModelState.IsValid)
                {
                    r.Estado = false;
                    r.Mensaje = JoinModelErrors(ModelState);
                    return BadRequest(r);
                }

                if (!string.IsNullOrWhiteSpace(vm.Estado) &&
                    !EstadosSolicitud.Todos.Contains(vm.Estado))
                {
                    r.Estado = false;
                    r.Mensaje = "Estado inválido.";
                    return BadRequest(r);
                }

                if (vm.FechaSolicitud == default)
                    vm.FechaSolicitud = DateTime.UtcNow;

                var creada = await _solicitudService.Crear(vm.ToEntity());
                var completa = await _solicitudService.ObtenerPorId(creada.Id);

                r.Estado = true;
                r.Objeto = (completa ?? creada).ToVM();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // 🔐 Editar (PUT JSON + AntiForgery)
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromBody] VMSolicitudesInstalacion vm)
        {
            var r = new GenericResponse<VMSolicitudesInstalacion>();
            try
            {
                if (!ModelState.IsValid)
                {
                    r.Estado = false;
                    r.Mensaje = JoinModelErrors(ModelState);
                    return BadRequest(r);
                }

                if (!string.IsNullOrWhiteSpace(vm.Estado) &&
                    !EstadosSolicitud.Todos.Contains(vm.Estado))
                {
                    r.Estado = false;
                    r.Mensaje = "Estado inválido.";
                    return BadRequest(r);
                }

                var actualizada = await _solicitudService.Editar(vm.ToEntity());
                var completa = await _solicitudService.ObtenerPorId(actualizada.Id);

                r.Estado = true;
                r.Objeto = (completa ?? actualizada).ToVM();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // 🔐 Eliminar (DELETE + AntiForgery)
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var r = new GenericResponse<object>();
            try
            {
                r.Estado = await _solicitudService.Eliminar(id);
                if (!r.Estado)
                    r.Mensaje = "Solicitud no encontrada o no se pudo eliminar.";
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