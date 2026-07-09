using EcoInstalacion.AppWeb.DTOs.Roles;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/roles")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public class RolesApiController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolesApiController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RolListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RolListDto>>> GetRoles(
            [FromQuery] string? filtro = null,
            [FromQuery] bool? soloActivos = null)
        {
            var roles = await _rolService.Lista(filtro, soloActivos);

            var result = roles.Select(r => new RolListDto
            {
                IdRol = r.IdRol,
                Descripcion = r.Descripcion ?? string.Empty,
                EsActivo = r.EsActivo ?? false
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RolDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDetailDto>> GetRolById(int id)
        {
            var rol = await _rolService.ObtenerPorId(id);

            if (rol is null)
                return NotFound(new { message = "Rol no encontrado." });

            var result = new RolDetailDto
            {
                IdRol = rol.IdRol,
                Descripcion = rol.Descripcion ?? string.Empty,
                EsActivo = rol.EsActivo ?? false,
                FechaRegistro = rol.FechaRegistro
            };

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RolDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RolDetailDto>> CreateRol([FromBody] RolCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entidad = new Rol
                {
                    Descripcion = dto.Descripcion.Trim(),
                    EsActivo = dto.EsActivo
                };

                var creado = await _rolService.Crear(entidad);

                var result = new RolDetailDto
                {
                    IdRol = creado.IdRol,
                    Descripcion = creado.Descripcion ?? string.Empty,
                    EsActivo = creado.EsActivo ?? false,
                    FechaRegistro = creado.FechaRegistro
                };

                return CreatedAtAction(nameof(GetRolById), new { id = result.IdRol }, result);
            }
            catch (TaskCanceledException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RolDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDetailDto>> UpdateRol(int id, [FromBody] RolUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var existente = await _rolService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Rol no encontrado." });

                existente.Descripcion = dto.Descripcion.Trim();
                existente.EsActivo = dto.EsActivo;

                var actualizado = await _rolService.Editar(existente);

                var result = new RolDetailDto
                {
                    IdRol = actualizado.IdRol,
                    Descripcion = actualizado.Descripcion ?? string.Empty,
                    EsActivo = actualizado.EsActivo ?? false,
                    FechaRegistro = actualizado.FechaRegistro
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(typeof(RolDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RolDetailDto>> CambiarEstadoRol(int id, [FromQuery] bool esActivo)
        {
            try
            {
                var existente = await _rolService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Rol no encontrado." });

                var ok = await _rolService.CambiarEstado(id, esActivo);

                if (!ok)
                    return BadRequest(new { message = "No se pudo cambiar el estado del rol." });

                var actualizado = await _rolService.ObtenerPorId(id);

                if (actualizado is null)
                    return NotFound(new { message = "Rol no encontrado." });

                var result = new RolDetailDto
                {
                    IdRol = actualizado.IdRol,
                    Descripcion = actualizado.Descripcion ?? string.Empty,
                    EsActivo = actualizado.EsActivo ?? false,
                    FechaRegistro = actualizado.FechaRegistro
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
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                var existente = await _rolService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Rol no encontrado." });

                var eliminado = await _rolService.Eliminar(id);

                if (!eliminado)
                    return NotFound(new { message = "Rol no encontrado." });

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
