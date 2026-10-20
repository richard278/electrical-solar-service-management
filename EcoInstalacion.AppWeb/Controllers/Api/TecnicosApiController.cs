using EcoInstalacion.AppWeb.DTOs.Shared;
using EcoInstalacion.AppWeb.DTOs.Tecnicos;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/tecnicos")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(Policy = "ApiAuthenticated")]
    public class TecnicosApiController : ControllerBase
    {
        private readonly ITecnicoService _tecnicoService;

        public TecnicosApiController(ITecnicoService tecnicoService)
        {
            _tecnicoService = tecnicoService;
        }

        private static TecnicoListDto MapToListDto(Tecnico t) => new()
        {
            Id = t.Id,
            Nombre = t.Nombre,
            Especialidad = t.Especialidad,
            Telefono = t.Telefono,
            Correo = t.Correo
        };

        [HttpGet]
        [ProducesResponseType(typeof(List<TecnicoListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TecnicoListDto>>> GetTecnicos()
        {
            var tecnicos = await _tecnicoService.Lista();
            return Ok(tecnicos.Select(MapToListDto).ToList());
        }

        [HttpGet("paginados")]
        [ProducesResponseType(typeof(PagedResponse<TecnicoListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<TecnicoListDto>>> GetTecnicosPaginados(
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _tecnicoService.ListaPaginada(
                q,
                page,
                pageSize);

            var response = new PagedResponse<TecnicoListDto>
            {
                Items = result.Items.Select(MapToListDto).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TecnicoDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TecnicoDetailDto>> GetTecnicoById(int id)
        {
            var tecnico = await _tecnicoService.ObtenerPorId(id);

            if (tecnico is null)
                return NotFound(new { message = "Técnico no encontrado." });

            var result = new TecnicoDetailDto
            {
                Id = tecnico.Id,
                Nombre = tecnico.Nombre,
                Especialidad = tecnico.Especialidad,
                Telefono = tecnico.Telefono,
                Correo = tecnico.Correo
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(TecnicoDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TecnicoDetailDto>> CreateTecnico([FromBody] TecnicoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entidad = new Tecnico
                {
                    Nombre = dto.Nombre.Trim(),
                    Especialidad = dto.Especialidad?.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Correo = dto.Correo?.Trim()
                };

                var creado = await _tecnicoService.Crear(entidad);

                var result = new TecnicoDetailDto
                {
                    Id = creado.Id,
                    Nombre = creado.Nombre,
                    Especialidad = creado.Especialidad,
                    Telefono = creado.Telefono,
                    Correo = creado.Correo
                };

                return CreatedAtAction(nameof(GetTecnicoById), new { id = result.Id }, result);
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
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(TecnicoDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TecnicoDetailDto>> UpdateTecnico(int id, [FromBody] TecnicoUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var existente = await _tecnicoService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Técnico no encontrado." });

                existente.Nombre = dto.Nombre.Trim();
                existente.Especialidad = dto.Especialidad?.Trim();
                existente.Telefono = dto.Telefono?.Trim();
                existente.Correo = dto.Correo?.Trim();

                var actualizado = await _tecnicoService.Editar(existente);

                var result = new TecnicoDetailDto
                {
                    Id = actualizado.Id,
                    Nombre = actualizado.Nombre,
                    Especialidad = actualizado.Especialidad,
                    Telefono = actualizado.Telefono,
                    Correo = actualizado.Correo
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
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTecnico(int id)
        {
            try
            {
                var existente = await _tecnicoService.ObtenerPorId(id);

                if (existente is null)
                    return NotFound(new { message = "Técnico no encontrado." });

                var eliminado = await _tecnicoService.Eliminar(id);

                if (!eliminado)
                    return NotFound(new { message = "Técnico no encontrado." });

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
