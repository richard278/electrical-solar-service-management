using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EcoInstalacion.AppWeb.DTOs.Reportes;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers.Api
{
    [ApiController]
    [Route("api/reportes")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [Authorize(Policy = "ApiAdminOnly")]
    public class ReportesApiController : ControllerBase
    {
        private const string IsoDateFormat = "yyyy-MM-dd";
        private const string PdfFileName = "Reporte_Tecnicos_Asignados.pdf";
        private const string PdfContentType = "application/pdf";

        private readonly IReporteService _reporteService;
        private readonly IReportePdfService _pdfService;
        private readonly IWebHostEnvironment? _env;

        public ReportesApiController(
            IReporteService reporteService,
            IReportePdfService pdfService,
            IWebHostEnvironment? env = null)
        {
            _reporteService = reporteService ?? throw new ArgumentNullException(nameof(reporteService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _env = env;
        }

        [HttpGet("tecnicos-asignados")]
        [ProducesResponseType(typeof(List<ReporteTecnicoAsignadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ReporteTecnicoAsignadoDto>>> GetTecnicosAsignados(
            [FromQuery] string? fechaInicio = null,
            [FromQuery] string? fechaFin = null)
        {
            if (!TryValidarRangoFechas(fechaInicio, fechaFin, out var fi, out var ff, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            var listaReporte = await _reporteService.ReporteTecnicosAsignados(fi, ff)
                ?? new List<ReporteTecnicosAsignados>();

            var result = listaReporte
                .Select(MapToDto)
                .ToList();

            return Ok(result);
        }

        [HttpGet("tecnicos-asignados/pdf")]
        [Produces(PdfContentType, "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportarTecnicosAsignadosPdf(
            [FromQuery] string? fechaInicio = null,
            [FromQuery] string? fechaFin = null)
        {
            if (!TryValidarRangoFechas(fechaInicio, fechaFin, out var fi, out var ff, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            var listaReporte = await _reporteService.ReporteTecnicosAsignados(fi, ff);

            if (listaReporte is null || listaReporte.Count == 0)
            {
                return NotFound(new
                {
                    message = "No hay datos disponibles para generar el PDF en el rango seleccionado."
                });
            }

            byte[]? logoBytes = await TryLoadLogoBytesAsync();

            var pdfBytes = _pdfService.GenerarReporteTecnicosAsignadosPDF(
                listaReporte,
                fi,
                ff,
                logoBytes);

            return File(pdfBytes, PdfContentType, PdfFileName);
        }

        private static bool TryValidarRangoFechas(
            string? fechaInicioRaw,
            string? fechaFinRaw,
            out DateTime? fechaInicio,
            out DateTime? fechaFin,
            out string errorMessage)
        {
            fechaInicio = null;
            fechaFin = null;
            errorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(fechaInicioRaw))
            {
                if (!TryParseIsoDate(fechaInicioRaw, out var parsedInicio))
                {
                    errorMessage = $"El parámetro 'fechaInicio' ('{fechaInicioRaw}') no es válido. Use el formato ISO '{IsoDateFormat}'.";
                    return false;
                }

                fechaInicio = parsedInicio;
            }

            if (!string.IsNullOrWhiteSpace(fechaFinRaw))
            {
                if (!TryParseIsoDate(fechaFinRaw, out var parsedFin))
                {
                    errorMessage = $"El parámetro 'fechaFin' ('{fechaFinRaw}') no es válido. Use el formato ISO '{IsoDateFormat}'.";
                    return false;
                }

                fechaFin = parsedFin;
            }

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
            {
                errorMessage = "El rango de fechas no es válido: 'fechaInicio' no puede ser mayor que 'fechaFin'.";
                return false;
            }

            return true;
        }

        private static bool TryParseIsoDate(string rawValue, out DateTime parsedDate)
        {
            return DateTime.TryParseExact(
                rawValue.Trim(),
                IsoDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedDate);
        }

        private async Task<byte[]?> TryLoadLogoBytesAsync()
        {
            if (string.IsNullOrWhiteSpace(_env?.WebRootPath))
            {
                return null;
            }

            var logoPath = Path.Combine(_env.WebRootPath, "img", "logo-eco.png");
            if (!System.IO.File.Exists(logoPath))
            {
                return null;
            }

            return await System.IO.File.ReadAllBytesAsync(logoPath);
        }

        private static ReporteTecnicoAsignadoDto MapToDto(ReporteTecnicosAsignados item)
        {
            return new ReporteTecnicoAsignadoDto
            {
                Tecnico = item.Tecnico ?? string.Empty,
                Especialidad = item.Especialidad ?? string.Empty,
                FechaSolicitud = item.FechaSolicitud,
                FechaVisita = item.FechaVisita,
                TipoInstalacion = item.TipoInstalacion ?? string.Empty,
                Descripcion = item.Descripcion ?? string.Empty,
                Estado = item.Estado ?? string.Empty,
                Observaciones = item.Observaciones ?? string.Empty,
                Aprobado = item.Aprobado
            };
        }
    }
}
