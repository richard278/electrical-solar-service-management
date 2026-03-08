using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Técnico")]
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;

        public ClientesController(IClienteService clienteService, IMapper mapper)
        {
            _clienteService = clienteService;
            _mapper = mapper;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var r = new GenericResponse<VMClientes>();
            try
            {
                var lista = await _clienteService.Lista();
                r.Estado = true;
                r.Lista = _mapper.Map<List<VMClientes>>(lista);
            }
            catch (Exception ex)
            {
                r.Estado = false;
                r.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] VMClientes vm)
        {
            var gResponse = new GenericResponse<VMClientes>();
            try
            {
                if (!ModelState.IsValid)
                {
                    gResponse.Estado = false;
                    gResponse.Mensaje = "Datos incompletos o inválidos.";
                    return BadRequest(gResponse);
                }

                var creado = await _clienteService.Crear(vm.ToEntity());
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
        public async Task<IActionResult> Editar([FromBody] VMClientes vm)
        {
            var gResponse = new GenericResponse<VMClientes>();
            try
            {
                var editado = await _clienteService.Editar(vm.ToEntity());
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
                gResponse.Estado = await _clienteService.Eliminar(id);
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