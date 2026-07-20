using EcoInstalacion.AppWeb.DTOs.Solicitudes;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/solicitudes")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(Policy = "ApiAuthenticated")]
    public class SolicitudesApiController : ControllerBase
    {
        private readonly ISolicitudService _solicitudService;

        public SolicitudesApiController(ISolicitudService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SolicitudListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SolicitudListDto>>> GetSolicitudes()
        {
            var solicitudes = await _solicitudService.Lista();
            var result = solicitudes.Select(ToListDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SolicitudDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SolicitudDetailDto>> GetSolicitudById(int id)
        {
            var solicitud = await _solicitudService.ObtenerPorId(id);
            if (solicitud is null)
                return NotFound(new { message = "Solicitud no encontrada." });

            return Ok(ToDetailDto(solicitud));
        }

        [HttpPost]
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(SolicitudDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SolicitudDetailDto>> CreateSolicitud([FromBody] SolicitudCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entidad = new SolicitudesInstalacion
                {
                    ClienteId = dto.ClienteId,
                    TipoInstalacion = dto.TipoInstalacion?.Trim(),
                    Descripcion = dto.Descripcion?.Trim(),
                    TecnicoAsignadoId = dto.TecnicoAsignadoId
                };

                var creada = await _solicitudService.Crear(entidad);
                var result = ToDetailDto(creada);
                return CreatedAtAction(nameof(GetSolicitudById), new { id = result.Id }, result);
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
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(SolicitudDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SolicitudDetailDto>> UpdateSolicitud(int id, [FromBody] SolicitudUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var existente = await _solicitudService.ObtenerPorId(id);
                if (existente is null)
                    return NotFound(new { message = "Solicitud no encontrada." });

                existente.ClienteId = dto.ClienteId;
                existente.TipoInstalacion = dto.TipoInstalacion?.Trim();
                existente.Descripcion = dto.Descripcion?.Trim();
                existente.Estado = dto.Estado?.Trim();
                existente.TecnicoAsignadoId = dto.TecnicoAsignadoId;

                var actualizada = await _solicitudService.Editar(existente);
                var result = ToDetailDto(actualizada);
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

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSolicitud(int id)
        {
            try
            {
                var existente = await _solicitudService.ObtenerPorId(id);
                if (existente is null)
                    return NotFound(new { message = "Solicitud no encontrada." });

                var eliminado = await _solicitudService.Eliminar(id);
                if (!eliminado)
                    return NotFound(new { message = "Solicitud no encontrada." });

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

        private static SolicitudListDto ToListDto(SolicitudesInstalacion solicitud)
        {
            return new SolicitudListDto
            {
                Id = solicitud.Id,
                FechaSolicitud = solicitud.FechaSolicitud,
                ClienteId = solicitud.ClienteId,
                ClienteNombre = solicitud.Cliente?.NombreCompleto,
                TipoInstalacion = solicitud.TipoInstalacion,
                Estado = solicitud.Estado,
                TecnicoAsignadoId = solicitud.TecnicoAsignadoId,
                TecnicoNombre = solicitud.TecnicoAsignado?.Nombre
            };
        }

        private static SolicitudDetailDto ToDetailDto(SolicitudesInstalacion solicitud)
        {
            return new SolicitudDetailDto
            {
                Id = solicitud.Id,
                FechaSolicitud = solicitud.FechaSolicitud,
                ClienteId = solicitud.ClienteId,
                ClienteNombre = solicitud.Cliente?.NombreCompleto,
                TipoInstalacion = solicitud.TipoInstalacion,
                Descripcion = solicitud.Descripcion,
                Estado = solicitud.Estado,
                TecnicoAsignadoId = solicitud.TecnicoAsignadoId,
                TecnicoNombre = solicitud.TecnicoAsignado?.Nombre
            };
        }
    }
}
