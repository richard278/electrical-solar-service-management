using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.AppWeb.Utilidades.Response;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;// ← ESTE es importante para imagenes


namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly IReporteService _reporteService;
        private readonly IReportePdfService _pdfService;
        private readonly IWebHostEnvironment _env;

        public ReportesController(
            IReporteService reporteService,
            IReportePdfService pdfService,
            IWebHostEnvironment env)
        {
            _reporteService = reporteService;
            _pdfService = pdfService;
            _env = env;
        }

        // 🔹 Vista del reporte de técnicos asignados
        // GET: /Reportes/ReporteTecnicos
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ReporteTecnicos()
        {
            return View();
        }
        // 🔹 Endpoint que consume el DataTable (AJAX)
        // GET: /Reportes/ListaTecnicosAsignados?fechaInicio=dd/MM/yyyy&fechaFin=dd/MM/yyyy
        [HttpGet]
        public async Task<IActionResult> ListaTecnicosAsignados(
            string? fechaInicio,
            string? fechaFin)
        {
            var gResponse = new GenericResponse<VMReporteTecnicosAsignados>();

            try
            {
                // 1) Parsear fechas (vienen como texto dd/MM/yyyy desde el datepicker)
                DateTime? fi = ParseFecha(fechaInicio);
                DateTime? ff = ParseFecha(fechaFin);

                // 2) Obtener datos desde la capa de negocio
                var listaReporte = await _reporteService
                    .ReporteTecnicosAsignados(fi, ff);

                // 3) Mapear Entity -> ViewModel (simple y explícito)
                gResponse.Estado = true;
                gResponse.Lista = listaReporte
                    .Select(r => new VMReporteTecnicosAsignados
                    {
                        Tecnico = r.Tecnico,
                        Especialidad = r.Especialidad,
                        FechaSolicitud = r.FechaSolicitud,
                        FechaVisita = r.FechaVisita,
                        TipoInstalacion = r.TipoInstalacion ?? string.Empty,
                        Descripcion = r.Descripcion ?? string.Empty,
                        Estado = r.Estado ?? string.Empty,
                        Observaciones = r.Observaciones,
                        Aprobado = r.Aprobado
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        //Helper privado para parsear fechas en formato dd/MM/yyyy (es-SV)
        private static DateTime? ParseFecha(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var culture = new CultureInfo("es-SV");
            const string formato = "dd/MM/yyyy";

            if (DateTime.TryParseExact(
                    valor,
                    formato,
                    culture,
                    DateTimeStyles.None,
                    out var fecha))
            {
                return fecha;
            }

            // Si no logra parsear exacto, intenta un parseo más tolerante
            if (DateTime.TryParse(valor, culture, DateTimeStyles.None, out fecha))
            {
                return fecha;
            }

            return null;
        }

        // ====================
        // EXPORTAR PDF
        // ====================
        [HttpGet]
        public async Task<IActionResult> ExportarTecnicosPDF(DateTime? fi, DateTime? ff)
        {
            // 1. Obtener los datos del reporte
            var lista = await _reporteService.ReporteTecnicosAsignados(fi, ff);

            if (lista == null || lista.Count == 0)
            {
                TempData["mensaje"] = "No hay datos disponibles para generar el PDF.";
                return RedirectToAction("ReporteTecnicos");
            }

            // 2. Convertir al modelo usado por el PDF
            IList<ReporteTecnicosAsignados> datosPDF = lista.Select(x => new ReporteTecnicosAsignados
            {
                Tecnico = x.Tecnico,
                Especialidad = x.Especialidad,
                FechaSolicitud = x.FechaSolicitud,
                FechaVisita = x.FechaVisita,
                TipoInstalacion = x.TipoInstalacion,
                Descripcion = x.Descripcion,
                Estado = x.Estado,
                Observaciones = x.Observaciones,
                Aprobado = x.Aprobado
            }).ToList();

            // 3. Cargar el logo desde wwwroot/img/logo-eco.png
            byte[]? logoBytes = null;
            var logoPath = Path.Combine(_env.WebRootPath, "img", "logo-eco.png");

            if (System.IO.File.Exists(logoPath))
            {
                logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
            }

            // 4. Generar PDF con QuestPDF
            var pdfBytes = _pdfService.GenerarReporteTecnicosAsignadosPDF(
                datosPDF,
                fi,
                ff,
                logoBytes  // ← 🔥 ahora sí va alineado con el motor PDF
            );

            // 5. Retornar archivo PDF
            return File(pdfBytes,
                "application/pdf",
                "Reporte_Tecnicos_Asignados.pdf");
        }
    }
}
