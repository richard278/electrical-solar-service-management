using EcoInstalacion.AppWeb.DTOs.VisitasTecnicas;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/visitas-tecnicas")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(Policy = "ApiAuthenticated")]
    public class VisitasTecnicasApiController : ControllerBase
    {
        private readonly IVisitaTecnicaService _visitaService;

        public VisitasTecnicasApiController(IVisitaTecnicaService visitaService)
        {
            _visitaService = visitaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VisitaTecnicaListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<VisitaTecnicaListDto>>> GetVisitas([FromQuery] int? solicitudId = null)
        {
            if (solicitudId.HasValue && solicitudId.Value <= 0)
                return BadRequest(new { message = "La solicitud indicada no es válida." });

            List<VisitasTecnica> visitas;
            if (solicitudId.HasValue)
            {
                visitas = await _visitaService.ListaPorSolicitud(solicitudId.Value);
            }
            else
            {
                visitas = await _visitaService.Lista();
            }

            var result = visitas.Select(ToListDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VisitaTecnicaDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitaTecnicaDetailDto>> GetVisitaById(int id)
        {
            var visita = await _visitaService.ObtenerPorId(id);
            if (visita is null)
                return NotFound(new { message = "Visita técnica no encontrada." });

            return Ok(ToDetailDto(visita));
        }

        [HttpPost]
        [Authorize(Policy = "ApiAdminOnly")]
        [ProducesResponseType(typeof(VisitaTecnicaDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VisitaTecnicaDetailDto>> CreateVisita([FromBody] VisitaTecnicaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entidad = new VisitasTecnica
                {
                    SolicitudId = dto.SolicitudId,
                    FechaVisita = dto.FechaVisita!.Value,
                    Observaciones = dto.Observaciones?.Trim(),
                    Aprobado = dto.Aprobado!.Value
                };

                var creada = await _visitaService.Crear(entidad);
                var completa = await _visitaService.ObtenerPorId(creada.Id) ?? creada;
                var result = ToDetailDto(completa);

                return CreatedAtAction(nameof(GetVisitaById), new { id = result.Id }, result);
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
        [ProducesResponseType(typeof(VisitaTecnicaDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitaTecnicaDetailDto>> UpdateVisita(int id, [FromBody] VisitaTecnicaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var existente = await _visitaService.ObtenerPorId(id);
                if (existente is null)
                    return NotFound(new { message = "Visita técnica no encontrada." });

                existente.FechaVisita = dto.FechaVisita!.Value;
                existente.Observaciones = dto.Observaciones?.Trim();
                existente.Aprobado = dto.Aprobado!.Value;

                var actualizada = await _visitaService.Editar(existente);
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
        public async Task<IActionResult> DeleteVisita(int id)
        {
            try
            {
                var existente = await _visitaService.ObtenerPorId(id);
                if (existente is null)
                    return NotFound(new { message = "Visita técnica no encontrada." });

                var eliminado = await _visitaService.Eliminar(id);
                if (!eliminado)
                    return BadRequest(new { message = "No se pudo eliminar la visita técnica." });

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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static VisitaTecnicaListDto ToListDto(VisitasTecnica visita)
        {
            return new VisitaTecnicaListDto
            {
                Id = visita.Id,
                SolicitudId = visita.SolicitudId,
                FechaVisita = visita.FechaVisita,
                Observaciones = visita.Observaciones,
                Aprobado = visita.Aprobado == true,
                ClienteNombre = visita.Solicitud?.Cliente?.NombreCompleto,
                TipoInstalacion = visita.Solicitud?.TipoInstalacion,
                EstadoSolicitud = visita.Solicitud?.Estado,
                TecnicoNombre = visita.Solicitud?.TecnicoAsignado?.Nombre
            };
        }

        private static VisitaTecnicaDetailDto ToDetailDto(VisitasTecnica visita)
        {
            return new VisitaTecnicaDetailDto
            {
                Id = visita.Id,
                SolicitudId = visita.SolicitudId,
                FechaVisita = visita.FechaVisita,
                Observaciones = visita.Observaciones,
                Aprobado = visita.Aprobado == true,
                ClienteNombre = visita.Solicitud?.Cliente?.NombreCompleto,
                TipoInstalacion = visita.Solicitud?.TipoInstalacion,
                EstadoSolicitud = visita.Solicitud?.Estado,
                TecnicoNombre = visita.Solicitud?.TecnicoAsignado?.Nombre
            };
        }
    }
}
