using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class VisitasTecnicasController : Controller
    {
        private readonly IVisitaTecnicaService _visitaService;
        private readonly ISolicitudService _solicitudService;

        public VisitasTecnicasController(
            IVisitaTecnicaService visitaService,
            ISolicitudService solicitudService)
        {
            _visitaService = visitaService;
            _solicitudService = solicitudService;
        }

        public IActionResult Index() => View();

        private static string JoinModelErrors(ModelStateDictionary modelState) =>
            string.Join(" | ", modelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage));

        // =========================================
        // LISTA (general o filtrada por solicitud)
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Lista(int? solicitudId)
        {
            var r = new GenericResponse<VMVisitasTecnicas>();
            try
            {
                IEnumerable<VisitasTecnica> data;

                if (solicitudId.HasValue && solicitudId.Value > 0)
                    data = await _visitaService.ListaPorSolicitud(solicitudId.Value);
                else
                    data = await _visitaService.Lista();

                r.Estado = true;
                r.Lista = data.Select(v => v.ToVM()).ToList();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // COMBOS (solicitudes para selects)
        // =========================================
        [HttpGet]
        public async Task<IActionResult> DataCombos()
        {
            var r = new GenericResponse<object>();
            try
            {
                var solicitudes = (await _solicitudService.Lista())
                    .Select(s => new
                    {
                        s.Id,
                        ClienteNombre = s.Cliente?.NombreCompleto ?? "(sin cliente)"
                    })
                    .OrderBy(x => x.Id)
                    .ToList();

                r.Estado = true;
                r.Objeto = solicitudes;
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // TOGGLE APROBADO
        // =========================================
        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAprobado([FromBody] ToggleAprobadoDTO dto)
        {
            var r = new GenericResponse<object>();
            try
            {
                if (dto is null || dto.Id <= 0)
                {
                    r.Estado = false;
                    r.Mensaje = "Datos inválidos.";
                    return BadRequest(r);
                }

                var visita = await _visitaService.ObtenerPorId(dto.Id);
                if (visita is null)
                {
                    r.Estado = false;
                    r.Mensaje = "La visita no existe.";
                    return StatusCode(StatusCodes.Status200OK, r);
                }

                visita.Aprobado = dto.Aprobado;
                await _visitaService.Editar(visita);

                r.Estado = true;
                r.Objeto = new { visita.Id, visita.Aprobado };
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // LISTA POR SOLICITUD (endpoint dedicado)
        // =========================================
        [HttpGet]
        public async Task<IActionResult> ListaPorSolicitud(int solicitudId)
        {
            var r = new GenericResponse<VMVisitasTecnicas>();
            try
            {
                var lista = await _visitaService.ListaPorSolicitud(solicitudId);
                r.Estado = true;
                r.Lista = lista.Select(v => v.ToVM()).ToList();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // CREAR
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] VMVisitasTecnicas vm)
        {
            var r = new GenericResponse<VMVisitasTecnicas>();
            try
            {
                if (vm == null)
                {
                    r.Estado = false;
                    r.Mensaje = "El objeto de visita (vm) es requerido.";
                    return BadRequest(r);
                }

                if (!ModelState.IsValid)
                {
                    r.Estado = false;
                    r.Mensaje = JoinModelErrors(ModelState);
                    return BadRequest(r);
                }

                // Validar que exista la solicitud asociada
                var solicitud = await _solicitudService.ObtenerPorId(vm.SolicitudId);
                if (solicitud is null)
                {
                    r.Estado = false;
                    r.Mensaje = "La solicitud asociada no existe.";
                    return BadRequest(r);
                }

                // Si por alguna razón viene FechaVisita sin valor, ponemos
                if (vm.FechaVisita == default)
                    vm.FechaVisita = DateTime.UtcNow;

                var creada = await _visitaService.Crear(vm.ToEntity());

                r.Estado = true;
                r.Objeto = creada.ToVM();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // EDITAR
        // =========================================
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromBody] VMVisitasTecnicas vm)
        {
            var r = new GenericResponse<VMVisitasTecnicas>();
            try
            {
                if (vm == null)
                {
                    r.Estado = false;
                    r.Mensaje = "El objeto de visita (vm) es requerido.";
                    return BadRequest(r);
                }

                if (!ModelState.IsValid)
                {
                    r.Estado = false;
                    r.Mensaje = JoinModelErrors(ModelState);
                    return BadRequest(r);
                }

                var editada = await _visitaService.Editar(vm.ToEntity());
                r.Estado = true;
                r.Objeto = editada.ToVM();
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        // =========================================
        // ELIMINAR
        // =========================================
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var r = new GenericResponse<object>();
            try
            {
                r.Estado = await _visitaService.Eliminar(id);
                if (!r.Estado)
                    r.Mensaje = "Visita técnica no encontrada o no se pudo eliminar.";
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }
    }

    public class ToggleAprobadoDTO
    {
        public int Id { get; set; }
        public bool Aprobado { get; set; }
    }
}
