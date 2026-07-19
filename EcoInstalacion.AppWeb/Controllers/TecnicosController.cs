using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    [AutoValidateAntiforgeryToken] // protege POST/PUT/DELETE leyendo el header RequestVerificationToken
    public class TecnicosController : BaseController
    {
        private readonly ITecnicoService _tecnicoService;
        private readonly IMapper _mapper;

        public TecnicosController(ITecnicoService tecnicoService, IMapper mapper)
        {
            _tecnicoService = tecnicoService;
            _mapper = mapper;
        }

        //Oculta el miembro heredado 'BaseController.Index()'.
        //Usa la palabra clave 'new' si se pretende ocultarlo intencionadamente.
        [HttpGet]
        public new IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var gResponse = new GenericResponse<VMTecnicos>();
            try
            {
                var lista = await _tecnicoService.Lista();
                gResponse.Estado = true;
                gResponse.Lista = lista.Select(t => t.ToVM()).ToList();
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] VMTecnicos vm)
        {
            var gResponse = new GenericResponse<VMTecnicos>();
            try
            {
                if (!ModelState.IsValid)
                {
                    gResponse.Estado = false;
                    gResponse.Mensaje = "Datos inválidos.";
                    return BadRequest(gResponse);
                }

                var creado = await _tecnicoService.Crear(vm.ToEntity());
                gResponse.Estado = true;
                gResponse.Objeto = creado.ToVM();
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromBody] VMTecnicos vm)
        {
            var gResponse = new GenericResponse<VMTecnicos>();
            try
            {
                var editado = await _tecnicoService.Editar(vm.ToEntity());
                gResponse.Estado = true;
                gResponse.Objeto = editado.ToVM();
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var gResponse = new GenericResponse<bool>();
            try
            {
                gResponse.Estado = await _tecnicoService.Eliminar(id);
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
