using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion
{
    public class ReporteExcelService : IReporteExcelService
    {
        private const string WorksheetName = "Técnicos asignados";
        private const string DateDisplayFormat = "dd/MM/yyyy";
        private const int TitleRow = 1;
        private const int PeriodRow = 2;
        private const int HeaderRow = 4;
        private const int FirstDataRow = 5;
        private const int TotalColumns = 9;

        private static readonly string[] ColumnHeaders =
        {
            "Técnico",
            "Especialidad",
            "Fecha solicitud",
            "Fecha visita",
            "Tipo instalación",
            "Descripción",
            "Estado",
            "Observaciones",
            "Aprobado"
        };

        private static readonly double[] ColumnWidths =
        {
            26, // 1. Técnico
            22, // 2. Especialidad
            16, // 3. Fecha solicitud
            16, // 4. Fecha visita
            18, // 5. Tipo instalación
            40, // 6. Descripción
            16, // 7. Estado
            40, // 8. Observaciones
            12  // 9. Aprobado
        };

        public byte[] GenerarReporteTecnicosAsignadosXlsx(
            IReadOnlyList<ReporteTecnicosAsignados> items,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var safeItems = items ?? Array.Empty<ReporteTecnicosAsignados>();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(WorksheetName);

            ComposeMetadataBlock(worksheet, fechaInicio, fechaFin);
            ComposeTableHeaders(worksheet);
            var lastRow = ComposeDataRows(worksheet, safeItems);

            worksheet.Range(HeaderRow, 1, lastRow, TotalColumns).SetAutoFilter();
            worksheet.SheetView.FreezeRows(HeaderRow);
            ApplyColumnWidths(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void ComposeMetadataBlock(
            IXLWorksheet worksheet,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var titleRange = worksheet.Range(TitleRow, 1, TitleRow, TotalColumns);
            titleRange.Merge();
            var titleCell = worksheet.Cell(TitleRow, 1);
            titleCell.SetValue("REPORTE DE TÉCNICOS ASIGNADOS — ECO INSTALACIÓN");
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            titleCell.Style.Font.FontColor = XLColor.FromHtml("#0B2545");
            titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(TitleRow).Height = 24;

            var periodRange = worksheet.Range(PeriodRow, 1, PeriodRow, TotalColumns);
            periodRange.Merge();
            var periodCell = worksheet.Cell(PeriodRow, 1);
            periodCell.SetValue($"Período consultado: {FormatPeriodo(fechaInicio, fechaFin)}");
            periodCell.Style.Font.Italic = true;
            periodCell.Style.Font.FontSize = 10;
            periodCell.Style.Font.FontColor = XLColor.FromHtml("#334155");
            periodCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(PeriodRow).Height = 18;
        }

        private static void ComposeTableHeaders(IXLWorksheet worksheet)
        {
            for (int colIndex = 0; colIndex < TotalColumns; colIndex++)
            {
                var cell = worksheet.Cell(HeaderRow, colIndex + 1);
                cell.SetValue(ColumnHeaders[colIndex]);
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B2545");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            }

            worksheet.Row(HeaderRow).Height = 22;
        }

        private static int ComposeDataRows(
            IXLWorksheet worksheet,
            IReadOnlyList<ReporteTecnicosAsignados> items)
        {
            if (items.Count == 0)
            {
                return HeaderRow;
            }

            int currentRow = FirstDataRow;

            foreach (var item in items)
            {
                SetLiteralTextCell(worksheet.Cell(currentRow, 1), item.Tecnico);
                SetLiteralTextCell(worksheet.Cell(currentRow, 2), item.Especialidad);

                var fechaSolicitudCell = worksheet.Cell(currentRow, 3);
                fechaSolicitudCell.SetValue(item.FechaSolicitud.Date);
                fechaSolicitudCell.Style.DateFormat.Format = DateDisplayFormat;
                fechaSolicitudCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var fechaVisitaCell = worksheet.Cell(currentRow, 4);
                if (item.FechaVisita.HasValue)
                {
                    fechaVisitaCell.SetValue(item.FechaVisita.Value.Date);
                    fechaVisitaCell.Style.DateFormat.Format = DateDisplayFormat;
                    fechaVisitaCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    fechaVisitaCell.Clear(XLClearOptions.Contents);
                }

                SetLiteralTextCell(worksheet.Cell(currentRow, 5), item.TipoInstalacion);
                SetLiteralTextCell(worksheet.Cell(currentRow, 6), item.Descripcion);
                SetLiteralTextCell(worksheet.Cell(currentRow, 7), item.Estado);
                SetLiteralTextCell(worksheet.Cell(currentRow, 8), item.Observaciones);

                var aprobadoCell = worksheet.Cell(currentRow, 9);
                aprobadoCell.SetValue(item.Aprobado);
                aprobadoCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var rowRange = worksheet.Range(currentRow, 1, currentRow, TotalColumns);
                rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                rowRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");
                rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                currentRow++;
            }

            return currentRow - 1;
        }

        private static void SetLiteralTextCell(IXLCell cell, string? rawText)
        {
            var value = rawText ?? string.Empty;
            cell.SetValue(value);

            if (StartsWithFormulaTrigger(value))
            {
                cell.Style.IncludeQuotePrefix = true;
            }
        }

        private static bool StartsWithFormulaTrigger(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            char firstChar = value[0];
            return firstChar is '=' or '+' or '-' or '@' or '\t' or '\r';
        }

        private static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            for (int i = 0; i < TotalColumns; i++)
            {
                worksheet.Column(i + 1).Width = ColumnWidths[i];
            }
        }

        private static string FormatPeriodo(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue && !fechaFin.HasValue)
            {
                return "Todos los registros";
            }

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                return $"{fechaInicio.Value:dd/MM/yyyy} → {fechaFin.Value:dd/MM/yyyy}";
            }

            if (fechaInicio.HasValue)
            {
                return $"Desde: {fechaInicio.Value:dd/MM/yyyy}";
            }

            return $"Hasta: {fechaFin!.Value:dd/MM/yyyy}";
        }
    }
}
