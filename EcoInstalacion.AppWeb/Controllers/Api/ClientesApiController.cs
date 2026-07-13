using EcoInstalacion.AppWeb.DTOs.Clientes;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/clientes")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(Policy = "ApiAuthenticated")]
    public class ClientesApiController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesApiController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ClienteListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClienteListDto>>> GetClientes()
        {
            var clientes = await _clienteService.Lista();

            var result = clientes.Select(c => new ClienteListDto
            {
                Id = c.Id,
                NombreCompleto = c.NombreCompleto,
                Dui = c.Dui,
                Telefono = c.Telefono,
                Correo = c.Correo,
                EsActivo = c.EsActivo
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteDetailDto>> GetClienteById(int id)
        {
            var cliente = await _clienteService.ObtenerPorId(id);

            if (cliente is null)
                return NotFound(new { message = "Cliente no encontrado." });

            var result = new ClienteDetailDto
            {
                Id = cliente.Id,
                NombreCompleto = cliente.NombreCompleto,
                Dui = cliente.Dui,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Correo = cliente.Correo,
                EsActivo = cliente.EsActivo
            };

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClienteDetailDto>> CreateCliente([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entidad = new Cliente
                {
                    NombreCompleto = dto.NombreCompleto.Trim(),
                    Dui = dto.Dui.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Direccion = dto.Direccion?.Trim(),
                    Correo = dto.Correo?.Trim(),
                    EsActivo = true
                };

                var creado = await _clienteService.Crear(entidad);

                var result = new ClienteDetailDto
                {
                    Id = creado.Id,
                    NombreCompleto = creado.NombreCompleto,
                    Dui = creado.Dui,
                    Telefono = creado.Telefono,
                    Direccion = creado.Direccion,
                    Correo = creado.Correo,
                    EsActivo = creado.EsActivo
                };

                return CreatedAtAction(nameof(GetClienteById), new { id = result.Id }, result);
            }
            catch (TaskCanceledException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteDetailDto>> UpdateCliente(int id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var existente = await _clienteService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Cliente no encontrado." });

                existente.NombreCompleto = dto.NombreCompleto.Trim();
                existente.Dui = dto.Dui.Trim();
                existente.Telefono = dto.Telefono?.Trim();
                existente.Direccion = dto.Direccion?.Trim();
                existente.Correo = dto.Correo?.Trim();
                existente.EsActivo = dto.EsActivo;

                var actualizado = await _clienteService.Editar(existente);

                var result = new ClienteDetailDto
                {
                    Id = actualizado.Id,
                    NombreCompleto = actualizado.NombreCompleto,
                    Dui = actualizado.Dui,
                    Telefono = actualizado.Telefono,
                    Direccion = actualizado.Direccion,
                    Correo = actualizado.Correo,
                    EsActivo = actualizado.EsActivo
                };

                return Ok(result);
            }
            catch (TaskCanceledException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var existente = await _clienteService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Cliente no encontrado." });

                var eliminado = await _clienteService.Eliminar(id);

                if (!eliminado)
                    return NotFound(new { message = "Cliente no encontrado." });

                return NoContent();
            }
            catch (TaskCanceledException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
