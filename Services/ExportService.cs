using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using PlasticQC.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlasticQC.Services
{
    public class ExportService
    {
        public async Task<bool> ExportRecordToPdfAsync(RecordListItem record, List<MeasurementDisplayItem> measurements)
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"Record_{record.ProductNumber}_{record.RecordDate:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(documentsPath, fileName);

                using (var document = new Document(PageSize.A4, 36, 36, 60, 36))
                {
                    using (var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                    {
                        document.Open();

                        // Add header
                        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                        var titleParagraph = new Paragraph($"Production Record: {record.ProductName}", headerFont);
                        titleParagraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(titleParagraph);
                        document.Add(new Paragraph(" "));

                        // Add record details
                        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                        var detailsTable = new PdfPTable(2);
                        detailsTable.WidthPercentage = 100;

                        AddCellWithNoBorder(detailsTable, "Product Number:", boldFont);
                        AddCellWithNoBorder(detailsTable, record.ProductNumber, normalFont);

                        AddCellWithNoBorder(detailsTable, "Machine:", boldFont);
                        AddCellWithNoBorder(detailsTable, record.MachineNumber, normalFont);

                        AddCellWithNoBorder(detailsTable, "Date:", boldFont);
                        AddCellWithNoBorder(detailsTable, record.RecordDate.ToString("d MMM yyyy HH:mm"), normalFont);

                        AddCellWithNoBorder(detailsTable, "Recorded by:", boldFont);
                        AddCellWithNoBorder(detailsTable, record.CreatedByName, normalFont);

                        AddCellWithNoBorder(detailsTable, "Quantity:", boldFont);
                        AddCellWithNoBorder(detailsTable, $"{record.QuantityMeasured} items", normalFont);

                        document.Add(detailsTable);
                        document.Add(new Paragraph(" "));

                        // Add measurements table
                        var measurementsTable = new PdfPTable(6);
                        measurementsTable.WidthPercentage = 100;

                        // Define column widths
                        float[] widths = new float[] { 10f, 15f, 20f, 15f, 20f, 20f };
                        measurementsTable.SetWidths(widths);

                        // Add header
                        var lightGray = new BaseColor(211, 211, 211);
                        var white = new BaseColor(255, 255, 255);

                        AddHeaderCell(measurementsTable, "Item #", boldFont, lightGray);
                        AddHeaderCell(measurementsTable, "Visual", boldFont, lightGray);
                        AddHeaderCell(measurementsTable, "Weight (g)", boldFont, lightGray);
                        AddHeaderCell(measurementsTable, "Height", boldFont, lightGray);
                        AddHeaderCell(measurementsTable, "Rim (mm)", boldFont, lightGray);
                        AddHeaderCell(measurementsTable, "Load (NM)", boldFont, lightGray);

                        // Add data rows
                        foreach (var measurement in measurements)
                        {
                            // Background color for alternating rows
                            var bgColor = measurement.ItemNumber % 2 == 0 ? new BaseColor(240, 240, 240) : white;

                            var itemCell = new PdfPCell(new Phrase(measurement.ItemNumber.ToString(), normalFont));
                            itemCell.BackgroundColor = bgColor;
                            itemCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(itemCell);

                            var visualCell = new PdfPCell(new Phrase(measurement.VisualLookOk ? "OK" : "FAIL", normalFont));
                            visualCell.BackgroundColor = bgColor;
                            visualCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(visualCell);

                            var weightText = $"{measurement.Weight:F1}\n(Std: {measurement.StandardWeight:F1})";
                            var weightCell = new PdfPCell(new Phrase(weightText, normalFont));
                            weightCell.BackgroundColor = bgColor;
                            weightCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(weightCell);

                            var heightCell = new PdfPCell(new Phrase(measurement.HeightOk ? "OK" : "FAIL", normalFont));
                            heightCell.BackgroundColor = bgColor;
                            heightCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(heightCell);

                            var rimText = $"{measurement.RimThickness:F1}\n(Std: {measurement.StandardRimThickness:F1})";
                            var rimCell = new PdfPCell(new Phrase(rimText, normalFont));
                            rimCell.BackgroundColor = bgColor;
                            rimCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(rimCell);

                            var loadText = $"{measurement.Load:F1}\n(Std: {measurement.StandardLoad:F1})";
                            var loadCell = new PdfPCell(new Phrase(loadText, normalFont));
                            loadCell.BackgroundColor = bgColor;
                            loadCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            measurementsTable.AddCell(loadCell);
                        }

                        document.Add(measurementsTable);

                        // Add summary
                        document.Add(new Paragraph(" "));
                        var summaryParagraph = new Paragraph("Summary:", boldFont);
                        document.Add(summaryParagraph);

                        var weightOutOfSpec = measurements.Count(m => !m.WeightInSpec);
                        var heightOutOfSpec = measurements.Count(m => !m.HeightOk);
                        var rimOutOfSpec = measurements.Count(m => !m.RimThicknessInSpec);
                        var loadOutOfSpec = measurements.Count(m => !m.LoadInSpec);
                        var visualOutOfSpec = measurements.Count(m => !m.VisualLookOk);

                        var summaryTable = new PdfPTable(2);
                        summaryTable.WidthPercentage = 100;

                        AddCellWithNoBorder(summaryTable, "Weight out of spec:", boldFont);
                        AddCellWithNoBorder(summaryTable, $"{weightOutOfSpec} of {measurements.Count}", normalFont);

                        AddCellWithNoBorder(summaryTable, "Height out of spec:", boldFont);
                        AddCellWithNoBorder(summaryTable, $"{heightOutOfSpec} of {measurements.Count}", normalFont);

                        AddCellWithNoBorder(summaryTable, "Rim thickness out of spec:", boldFont);
                        AddCellWithNoBorder(summaryTable, $"{rimOutOfSpec} of {measurements.Count}", normalFont);

                        AddCellWithNoBorder(summaryTable, "Load out of spec:", boldFont);
                        AddCellWithNoBorder(summaryTable, $"{loadOutOfSpec} of {measurements.Count}", normalFont);

                        AddCellWithNoBorder(summaryTable, "Visual appearance issues:", boldFont);
                        AddCellWithNoBorder(summaryTable, $"{visualOutOfSpec} of {measurements.Count}", normalFont);

                        document.Add(summaryTable);

                        // Add footer with date and page numbers
                        var footer = new Paragraph($"Generated on {DateTime.Now:d MMM yyyy HH:mm:ss}", FontFactory.GetFont(FontFactory.HELVETICA, 8));
                        footer.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                        document.Add(footer);
                    }
                }

                // Try to open the file
                await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Microsoft.Maui.ApplicationModel.OpenFileRequest
                {
                    File = new Microsoft.Maui.Storage.ReadOnlyFile(filePath)
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to PDF: {ex.Message}");
                return false;
            }
        }

        private void AddCellWithNoBorder(PdfPTable table, string text, iTextSharp.text.Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.Border = 0;
            table.AddCell(cell);
        }

        private void AddHeaderCell(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor bgColor)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(cell);
        }

        public async Task<bool> ExportToExcelAsync(RecordListItem record, List<MeasurementDisplayItem> measurements)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"Record_{record.ProductNumber}_{record.RecordDate:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(documentsPath, fileName);

                using (var package = new ExcelPackage())
                {
                    // Add a worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Production Record");

                    // Add title
                    worksheet.Cells[1, 1].Value = $"Production Record: {record.ProductName}";
                    worksheet.Cells[1, 1, 1, 6].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Add record details
                    worksheet.Cells[3, 1].Value = "Product Number:";
                    worksheet.Cells[3, 2].Value = record.ProductNumber;
                    worksheet.Cells[4, 1].Value = "Machine:";
                    worksheet.Cells[4, 2].Value = record.MachineNumber;
                    worksheet.Cells[5, 1].Value = "Date:";
                    worksheet.Cells[5, 2].Value = record.RecordDate;
                    worksheet.Cells[5, 2].Style.Numberformat.Format = "dd MMM yyyy HH:mm";
                    worksheet.Cells[6, 1].Value = "Recorded by:";
                    worksheet.Cells[6, 2].Value = record.CreatedByName;
                    worksheet.Cells[7, 1].Value = "Quantity:";
                    worksheet.Cells[7, 2].Value = $"{record.QuantityMeasured} items";

                    // Format details
                    worksheet.Cells[3, 1, 7, 1].Style.Font.Bold = true;

                    // Add measurements table header
                    int rowIndex = 9;
                    worksheet.Cells[rowIndex, 1].Value = "Item #";
                    worksheet.Cells[rowIndex, 2].Value = "Visual";
                    worksheet.Cells[rowIndex, 3].Value = "Weight (g)";
                    worksheet.Cells[rowIndex, 4].Value = "Height";
                    worksheet.Cells[rowIndex, 5].Value = "Rim (mm)";
                    worksheet.Cells[rowIndex, 6].Value = "Load (NM)";

                    // Format header
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Font.Bold = true;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(200, 200, 200));
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Add measurements data
                    rowIndex++;
                    foreach (var measurement in measurements)
                    {
                        worksheet.Cells[rowIndex, 1].Value = measurement.ItemNumber;
                        worksheet.Cells[rowIndex, 2].Value = measurement.VisualLookOk ? "OK" : "FAIL";
                        worksheet.Cells[rowIndex, 3].Value = measurement.Weight;
                        worksheet.Cells[rowIndex, 4].Value = measurement.HeightOk ? "OK" : "FAIL";
                        worksheet.Cells[rowIndex, 5].Value = measurement.RimThickness;
                        worksheet.Cells[rowIndex, 6].Value = measurement.Load;

                        // Add conditional formatting for out of spec values
                        if (!measurement.WeightInSpec)
                        {
                            worksheet.Cells[rowIndex, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }

                        if (!measurement.HeightOk)
                        {
                            worksheet.Cells[rowIndex, 4].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }

                        if (!measurement.RimThicknessInSpec)
                        {
                            worksheet.Cells[rowIndex, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }

                        if (!measurement.LoadInSpec)
                        {
                            worksheet.Cells[rowIndex, 6].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }

                        if (!measurement.VisualLookOk)
                        {
                            worksheet.Cells[rowIndex, 2].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }

                        rowIndex++;
                    }

                    // Add standard values
                    rowIndex++;
                    worksheet.Cells[rowIndex, 1].Value = "Standard Values";
                    worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                    rowIndex++;

                    if (measurements.Count > 0)
                    {
                        var firstMeasurement = measurements.First();

                        worksheet.Cells[rowIndex, 1].Value = "Standard:";
                        worksheet.Cells[rowIndex, 3].Value = firstMeasurement.StandardWeight;
                        worksheet.Cells[rowIndex, 5].Value = firstMeasurement.StandardRimThickness;
                        worksheet.Cells[rowIndex, 6].Value = firstMeasurement.StandardLoad;

                        rowIndex++;
                        worksheet.Cells[rowIndex, 1].Value = "Tolerance +";
                        worksheet.Cells[rowIndex, 3].Value = firstMeasurement.WeightTolerancePlus;
                        worksheet.Cells[rowIndex, 5].Value = firstMeasurement.RimThicknessTolerancePlus;
                        worksheet.Cells[rowIndex, 6].Value = firstMeasurement.LoadTolerancePlus;

                        rowIndex++;
                        worksheet.Cells[rowIndex, 1].Value = "Tolerance -";
                        worksheet.Cells[rowIndex, 3].Value = firstMeasurement.WeightToleranceMinus;
                        worksheet.Cells[rowIndex, 5].Value = firstMeasurement.RimThicknessToleranceMinus;
                        worksheet.Cells[rowIndex, 6].Value = firstMeasurement.LoadToleranceMinus;
                    }

                    // Add summary
                    rowIndex += 2;
                    worksheet.Cells[rowIndex, 1].Value = "Summary";
                    worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                    rowIndex++;

                    var weightOutOfSpec = measurements.Count(m => !m.WeightInSpec);
                    var heightOutOfSpec = measurements.Count(m => !m.HeightOk);
                    var rimOutOfSpec = measurements.Count(m => !m.RimThicknessInSpec);
                    var loadOutOfSpec = measurements.Count(m => !m.LoadInSpec);
                    var visualOutOfSpec = measurements.Count(m => !m.VisualLookOk);

                    worksheet.Cells[rowIndex, 1].Value = "Weight out of spec:";
                    worksheet.Cells[rowIndex, 2].Value = $"{weightOutOfSpec} of {measurements.Count}";
                    rowIndex++;

                    worksheet.Cells[rowIndex, 1].Value = "Height out of spec:";
                    worksheet.Cells[rowIndex, 2].Value = $"{heightOutOfSpec} of {measurements.Count}";
                    rowIndex++;

                    worksheet.Cells[rowIndex, 1].Value = "Rim thickness out of spec:";
                    worksheet.Cells[rowIndex, 2].Value = $"{rimOutOfSpec} of {measurements.Count}";
                    rowIndex++;

                    worksheet.Cells[rowIndex, 1].Value = "Load out of spec:";
                    worksheet.Cells[rowIndex, 2].Value = $"{loadOutOfSpec} of {measurements.Count}";
                    rowIndex++;

                    worksheet.Cells[rowIndex, 1].Value = "Visual appearance issues:";
                    worksheet.Cells[rowIndex, 2].Value = $"{visualOutOfSpec} of {measurements.Count}";

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Save the workbook
                    File.WriteAllBytes(filePath, package.GetAsByteArray());
                }

                // Try to open the file
                await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Microsoft.Maui.ApplicationModel.OpenFileRequest
                {
                    File = new Microsoft.Maui.Storage.ReadOnlyFile(filePath)
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                return false;
            }
        }
    }
}