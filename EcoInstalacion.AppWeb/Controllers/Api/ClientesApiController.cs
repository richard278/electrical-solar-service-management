using EcoInstalacion.AppWeb.DTOs.Clientes;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using EcoInstalacion.Entity.Seguridad;
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

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static ClienteListDto MapToListDto(Cliente c) => new()
        {
            Id             = c.Id,
            NombreCompleto = c.NombreCompleto,
            Dui            = c.Dui,
            Telefono       = c.Telefono,
            Direccion      = c.Direccion,
            Correo         = c.Correo,
            EsActivo       = c.EsActivo
        };

        private static ClienteDetailDto MapToDetailDto(Cliente c) => new()
        {
            Id             = c.Id,
            NombreCompleto = c.NombreCompleto,
            Dui            = c.Dui,
            Telefono       = c.Telefono,
            Direccion      = c.Direccion,
            Correo         = c.Correo,
            EsActivo       = c.EsActivo
        };

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/clientes — clientes activos (Administrador y Técnico)
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet]
        [ProducesResponseType(typeof(List<ClienteListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClienteListDto>>> GetClientes()
        {
            var clientes = await _clienteService.Lista();
            return Ok(clientes.Select(MapToListDto).ToList());
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/clientes/administracion?estado=activos|inactivos|todos
        // Solo Administrador (ApiAdminOnly)
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("administracion")]
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(List<ClienteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ClienteListDto>>> GetClientesAdministracion(
            [FromQuery] string estado = "activos")
        {
            var estadoNorm = estado.Trim().ToLowerInvariant();

            if (estadoNorm != "activos" && estadoNorm != "inactivos" && estadoNorm != "todos")
            {
                return BadRequest(new
                {
                    message = $"Valor de estado inválido: '{estado}'. Los valores aceptados son: activos, inactivos, todos."
                });
            }

            bool? esActivo = estadoNorm switch
            {
                "activos"   => true,
                "inactivos" => false,
                _           => (bool?)null
            };

            var clientes = await _clienteService.ListaPorEstado(esActivo);
            return Ok(clientes.Select(MapToListDto).ToList());
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/clientes/{id}
        // Administrador: activo e inactivo.
        // Técnico: solo activo (inactivo → 404 para no revelar existencia).
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteDetailDto>> GetClienteById(int id)
        {
            var cliente = await _clienteService.ObtenerPorId(id);

            if (cliente is null)
                return NotFound(new { message = "Cliente no encontrado." });

            // Si el cliente está inactivo y el usuario no es Administrador: 404
            if (!cliente.EsActivo && !User.IsInRole(RolesSistema.Administrador))
                return NotFound(new { message = "Cliente no encontrado." });

            return Ok(MapToDetailDto(cliente));
        }

        // ──────────────────────────────────────────────────────────────────────
        // POST /api/clientes — Administrador y Técnico. EsActivo = true siempre.
        // ──────────────────────────────────────────────────────────────────────

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
                    Dui            = dto.Dui.Trim(),
                    Telefono       = dto.Telefono?.Trim(),
                    Direccion      = dto.Direccion?.Trim(),
                    Correo         = dto.Correo?.Trim(),
                    EsActivo       = true
                };

                var creado = await _clienteService.Crear(entidad);
                return CreatedAtAction(nameof(GetClienteById), new { id = creado.Id }, MapToDetailDto(creado));
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

        // ──────────────────────────────────────────────────────────────────────
        // PUT /api/clientes/{id} — Solo Administrador
        // ──────────────────────────────────────────────────────────────────────

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ApiAdminOnly")]
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
                existente.Dui            = dto.Dui.Trim();
                existente.Telefono       = dto.Telefono?.Trim();
                existente.Direccion      = dto.Direccion?.Trim();
                existente.Correo         = dto.Correo?.Trim();
                existente.EsActivo       = dto.EsActivo;

                var actualizado = await _clienteService.Editar(existente);
                return Ok(MapToDetailDto(actualizado));
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

        // ──────────────────────────────────────────────────────────────────────
        // DELETE /api/clientes/{id} — Solo Administrador
        // ──────────────────────────────────────────────────────────────────────

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "ApiAdminOnly")]
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
