using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SICPAI.Security;

namespace SICPAI.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _db;

        public ReportesController(AppDbContext db) => _db = db;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/reportes/inventario/excel")]
        [PermissionAuthorize("reports.export")]
        public async Task<IActionResult> InventoryExcel([FromQuery] string? status, [FromQuery] string? area)
        {
            var assets = await QueryAssets(status, area);
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Inventario");
            var headers = new[] { "Código", "Bien", "Tipo", "Serie", "Área", "Responsable", "Ubicación", "Estado", "Valor" };
            for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
            for (var row = 0; row < assets.Count; row++)
            {
                var asset = assets[row]; var values = new object?[] { asset.AssetCode, asset.Name, asset.AssetType, asset.SerialNumber, asset.Area, asset.Responsible, asset.Location, asset.Status, asset.AcquisitionValue };
                for (var col = 0; col < values.Length; col++) sheet.Cell(row + 2, col + 1).Value = XLCellValue.FromObject(values[col]);
            }
            var header = sheet.Range(1, 1, 1, headers.Length); header.Style.Font.Bold = true; header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d47a1"); header.Style.Font.FontColor = XLColor.White;
            sheet.Columns().AdjustToContents(); sheet.SheetView.FreezeRows(1); sheet.RangeUsed()?.SetAutoFilter();
            using var stream = new MemoryStream(); workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"inventario-{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
        }

        [HttpGet("/api/reportes/inventario/pdf")]
        [PermissionAuthorize("reports.export")]
        public async Task<IActionResult> InventoryPdf([FromQuery] string? status, [FromQuery] string? area)
        {
            var assets = await QueryAssets(status, area);
            var document = Document.Create(container => container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); page.Margin(24); page.DefaultTextStyle(x => x.FontSize(8));
                page.Header().Column(column => { column.Item().Text("SICPAI").FontSize(18).Bold().FontColor(Colors.Blue.Darken3); column.Item().Text("Inventario institucional").FontSize(12).Bold(); column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"); });
                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns => { for (var i = 0; i < 7; i++) columns.RelativeColumn(); });
                    foreach (var header in new[] { "Código", "Bien", "Área", "Responsable", "Ubicación", "Estado", "Valor" }) table.Cell().Background(Colors.Blue.Darken3).Padding(4).Text(header).FontColor(Colors.White).Bold();
                    foreach (var asset in assets) { table.Cell().Padding(3).Text(asset.AssetCode); table.Cell().Padding(3).Text(asset.Name); table.Cell().Padding(3).Text(asset.Area ?? "-"); table.Cell().Padding(3).Text(asset.Responsible ?? "-"); table.Cell().Padding(3).Text(asset.Location ?? "-"); table.Cell().Padding(3).Text(asset.Status); table.Cell().Padding(3).Text(asset.AcquisitionValue.ToString("C")); }
                });
                page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
            }));
            return File(document.GeneratePdf(), "application/pdf", $"inventario-{DateTime.UtcNow:yyyyMMddHHmm}.pdf");
        }

        private async Task<List<Domain.Entities.Asset>> QueryAssets(string? status, string? area)
        {
            var query = _db.Assets.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
            if (!string.IsNullOrWhiteSpace(area)) query = query.Where(a => a.Area == area);
            return await query.OrderBy(a => a.AssetCode).ToListAsync();
        }

        public IActionResult Financiero()
        {
            return View();
        }

        public IActionResult Analytics()
        {
            return View();
        }
    }
}
