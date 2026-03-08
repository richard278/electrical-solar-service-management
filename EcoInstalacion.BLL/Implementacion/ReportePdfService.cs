using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace EcoInstalacion.BLL.Implementacion
{
    public class ReportePdfService : IReportePdfService
    {
        public byte[] GenerarReporteTecnicosAsignadosPDF(
            IList<ReporteTecnicosAsignados>
            items,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            byte[]? logoBytes = null)
        {
            var documento = new DocumentoTecnicosAsignados(
                items,
                fechaInicio,
                fechaFin,
                logoBytes
            );
            return documento.GeneratePdf();
        }
    }

    // ==========================
    //   DOCUMENTO QUESTPDF
    // ==========================
    public class DocumentoTecnicosAsignados : IDocument
    {
        private readonly IList<ReporteTecnicosAsignados> _items;
        private readonly DateTime? _fi;
        private readonly DateTime? _ff;

        // KPI
        private readonly int _total;
        private readonly int _aprobadas;
        private readonly double _porcentajeAprobadas;
        private readonly List<(string Tecnico, int Conteo)> _conteoPorTecnico;

        // Logo
        private readonly string? _logoPath;
        private readonly bool _logoExiste;

        public DocumentoTecnicosAsignados(
            IList<ReporteTecnicosAsignados> items,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            byte[]? logoBytes)
        {
            _items = items ?? new List<ReporteTecnicosAsignados>();
            _fi = fechaInicio;
            _ff = fechaFin;

            _total = _items.Count;
            _aprobadas = _items.Count(x => x.Aprobado);
            _porcentajeAprobadas = _total > 0 ? (_aprobadas * 100.0 / _total) : 0.0;

            _conteoPorTecnico = _items
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Tecnico) ? "Sin técnico" : x.Tecnico!)
                .Select(g => (Tecnico: g.Key, Conteo: g.Count()))
                .OrderByDescending(x => x.Conteo)
                .ToList();

            var baseDir = Directory.GetCurrentDirectory();
            _logoPath = Path.Combine(baseDir, "wwwroot", "img", "eco-logo.png");
            _logoExiste = File.Exists(_logoPath);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            // ✅ NO encadenar Page(), se llaman una tras otra
            container.Page(ComposePortada);             // primera página
            container.Page(ComposeContenidoPrincipal);  // páginas de contenido
        }

        // ==========================
        //       PORTADA
        // ==========================

        private void ComposePortada(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(40);

            page.Content().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("ECO INSTALACIÓN")
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        c.Item().Text("Plataforma de gestión para instalaciones responsables con el ambiente.")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(90).Height(90).Column(c =>
                    {
                        if (_logoExiste && _logoPath != null)
                        {
                            c.Item().AlignCenter().AlignMiddle().Image(_logoPath);
                        }
                        else
                        {
                            c.Item().AlignCenter().AlignMiddle()
                                .Background(Colors.Green.Lighten3)
                                .Border(1).BorderColor(Colors.Green.Darken1)
                                .Padding(5)
                                .Text("ECO")
                                .SemiBold()
                                .FontSize(22)
                                .FontColor(Colors.Green.Darken2);
                        }
                    });
                });

                col.Item().PaddingTop(30).AlignCenter().Text("REPORTE DE TÉCNICOS ASIGNADOS")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                col.Item().AlignCenter().Text(txt =>
                {
                    txt.Span("Generado el: ").SemiBold();
                    txt.Span($"{DateTime.Now:dd 'de' MMMM 'de' yyyy}");
                });

                col.Item().AlignCenter().Text(txt =>
                {
                    txt.Span("Rango de datos: ").SemiBold();
                    txt.Span(
                        (!_fi.HasValue && !_ff.HasValue) ? "Todos los registros" :
                        (_fi.HasValue && _ff.HasValue) ? $"{_fi:dd/MM/yyyy} → {_ff:dd/MM/yyyy}" :
                        _fi.HasValue ? $"Desde: {_fi:dd/MM/yyyy}" :
                                       $"Hasta: {_ff:dd/MM/yyyy}"
                    );
                });

                col.Item().PaddingTop(30).AlignCenter()
                    .Text("«Por un futuro amigable con el medio ambiente»")
                    .FontSize(14)
                    .Italic()
                    .FontColor(Colors.Green.Darken1);

                col.Item().AlignCenter()
                    .Text("Agregando cuidado a tu vida.")
                    .FontSize(12)
                    .FontColor(Colors.Grey.Darken2);

                col.Item().PaddingTop(50).Row(r =>
                {
                    r.RelativeItem().LineHorizontal(3).LineColor(Colors.Green.Medium);
                    r.ConstantItem(40).Height(0);
                });
            });
        }

        // ==========================
        //   PÁGINAS PRINCIPALES
        // ==========================
        private void ComposeContenidoPrincipal(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Content().Row(row =>
            {
                // banda lateral verde
                row.ConstantItem(6)
                    .ExtendVertical()
                    .Background(Colors.Green.Medium);

                row.RelativeItem().PaddingLeft(10).Column(col =>
                {
                    col.Item().Element(ComposeHeader);
                    col.Item().PaddingTop(10).Element(ComposeKpiResumen);
                    col.Item().PaddingTop(15).Element(ComposeTable);
                });
            });

            page.Footer()
                .AlignCenter()
                .Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
        }

        // ====== HEADER DE CONTENIDO ======
        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Reporte Técnicos Asignados")
                        .FontSize(14)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);
                        
                    col.Item().Text(txt =>
                    {
                        // Estilo por defecto para todo el bloque de texto
                        txt.DefaultTextStyle(s => s.FontColor(Colors.Grey.Darken1));

                        txt.Span("Rango: ")
                            .SemiBold();

                        txt.Span(
                            (!_fi.HasValue && !_ff.HasValue) ? "Todos los registros" :
                            (_fi.HasValue && _ff.HasValue) ? $"{_fi:dd/MM/yyyy} → {_ff:dd/MM/yyyy}" :
                            _fi.HasValue ? $"Desde: {_fi:dd/MM/yyyy}" :
                                           $"Hasta: {_ff:dd/MM/yyyy}"
                        );
                    });
                });
            });
        }

        // ====== KPIs / RESUMEN ======
        private void ComposeKpiResumen(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Green.Lighten2)
                    .Padding(6)
                    .Background(Colors.Green.Lighten5)
                    .Column(col =>
                    {
                        col.Item().Text("Resumen general")
                            .FontSize(11)
                            .SemiBold()
                            .FontColor(Colors.Green.Darken2);

                        col.Item().Text($"Total de filas: {_total}");
                        col.Item().Text($"Visitas aprobadas: {_aprobadas} ({_porcentajeAprobadas:F1} %)");
                    });

                row.RelativeItem().PaddingLeft(5).Column(col =>
                {
                    col.Item().Text("Visitas por técnico")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    foreach (var t in _conteoPorTecnico)
                        col.Item().Text($"• {t.Tecnico}: {t.Conteo} visita(s)");

                    if (_conteoPorTecnico.Count == 0)
                        col.Item().Text("Sin datos disponibles.");
                });
            });
        }

        // ====== TABLA DETALLE ======
        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(18); // #
                    cols.RelativeColumn(2);  // Técnico
                    cols.RelativeColumn(2);  // Especialidad
                    cols.RelativeColumn(2);  // Fecha solicitud
                    cols.RelativeColumn(2);  // Fecha visita
                    cols.RelativeColumn(2);  // Tipo instalación
                    cols.RelativeColumn(3);  // Descripción
                    cols.RelativeColumn(2);  // Estado
                    cols.RelativeColumn(3);  // Observaciones
                    cols.ConstantColumn(25); // Aprob.
                });

                // ENCABEZADOS
                table.Header(h =>
                {
                    void Head(IContainer cell, string txt) =>
                        cell.Background(Colors.Blue.Darken2)
                            .Padding(3)
                            .DefaultTextStyle(x => x
                                .FontColor(Colors.White)
                                .SemiBold()
                                .FontSize(9))
                            .Text(txt);

                    Head(h.Cell(), "#");
                    Head(h.Cell(), "Técnico");
                    Head(h.Cell(), "Especialidad");
                    Head(h.Cell(), "Fecha solicitud");
                    Head(h.Cell(), "Fecha visita");
                    Head(h.Cell(), "Tipo instalación");
                    Head(h.Cell(), "Descripción");
                    Head(h.Cell(), "Estado");
                    Head(h.Cell(), "Observaciones");
                    Head(h.Cell(), "Aprob.");
                });

                // FILAS
                int index = 1;

                foreach (var x in _items)
                {
                    table.Cell().Text(index.ToString());
                    table.Cell().Text(x.Tecnico ?? string.Empty);
                    table.Cell().Text(x.Especialidad ?? string.Empty);
                    table.Cell().Text(x.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Text(x.FechaVisita?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                    table.Cell().Text(x.TipoInstalacion ?? string.Empty);
                    table.Cell().Text(x.Descripcion ?? string.Empty);
                    table.Cell().Text(x.Estado ?? string.Empty);
                    table.Cell().Text(x.Observaciones ?? string.Empty);

                    table.Cell().Element(cell =>
                    {
                        bool ok = x.Aprobado;
                        string t = ok ? "Sí" : "No";
                        string color = ok ? Colors.Green.Medium : Colors.Red.Medium;

                        cell.Background(color)
                            .Padding(3)
                            .AlignCenter()
                            .DefaultTextStyle(s => s
                                .FontColor(Colors.White)
                                .SemiBold()
                                .FontSize(9))
                            .Text(t);
                    });

                    index++;
                }

                if (_items.Count == 0)
                {
                    table.Cell().ColumnSpan(10).Padding(5)
                        .AlignCenter()
                        .Text("No hay registros para el rango seleccionado.")
                        .FontColor(Colors.Grey.Darken1);
                }
            });
        }
    }
}
