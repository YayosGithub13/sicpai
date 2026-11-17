using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;
using Domain.DTOs;
using Application.Interfaces;
using System.Security.Claims;
using ClosedXML.Excel;
using SICPAI.Security;


namespace SICPAI.Controllers
{
    [Authorize]
    public class PatrimonioController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IAssetService _assetService;

        public PatrimonioController(IWebHostEnvironment env, IAssetService assetService)
        {
            _env = env;
            _assetService = assetService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/assets")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? area)
        {
            var assets = await _assetService.ListAsync(search, status, area, HttpContext.RequestAborted);
            return Ok(assets);
        }

        [HttpGet("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> Get(int id)
        {
            var asset = await _assetService.GetAsync(id, HttpContext.RequestAborted);
            return asset == null ? NotFound() : Ok(asset);
        }

        [HttpGet("/api/assets/scan/{code}")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> Scan(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest(new { error = "Código requerido." });
            var asset = await _assetService.ScanAsync(code, HttpContext.RequestAborted);
            return asset == null ? NotFound(new { error = "No se encontró un bien con ese código." }) : Ok(asset);
        }

        [HttpGet("/api/assets/movements")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> Movements([FromQuery] string? search, [FromQuery] string? type, DateTime? from, DateTime? to, int page = 1, int pageSize = 25)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await _assetService.ListMovementsAsync(search, type, from, to, page, pageSize, HttpContext.RequestAborted);
            var items = result.Items.Select(m => new
            {
                m.Id,
                m.OccurredAt,
                m.MovementType,
                m.Description,
                m.FromArea,
                m.ToArea,
                Asset = new { assetId = m.AssetId, assetCode = m.AssetCode, name = m.AssetName },
                User = m.UserId.HasValue ? new { id = m.UserId, fullName = m.UserFullName } : null
            });
            return Ok(new { page, pageSize, total = result.Total, items });
        }

        [HttpPost("/api/assets")]
        [PermissionAuthorize("assets.create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsset([FromBody] AssetRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.AssetCode) || string.IsNullOrWhiteSpace(request.Name))
                return ValidationProblem(ModelState);
            try
            {
                var asset = await _assetService.CreateAsync(request.ToDto(), CurrentUserId(), HttpContext.RequestAborted);
                return CreatedAtAction(nameof(Get), new { id = asset.Id }, asset);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { error = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { error = exception.Message });
            }
        }

        [HttpPut("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetRequest request)
        {
            try
            {
                var asset = await _assetService.UpdateAsync(id, request.ToDto(), CurrentUserId(), HttpContext.RequestAborted);
                return asset is null ? NotFound() : Ok(asset);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { error = exception.Message });
            }
        }

        [HttpPost("/api/assets/{id:int}/movements")]
        [PermissionAuthorize("assets.update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovement(int id, [FromBody] MovementRequest request)
        {
            var movement = await _assetService.AddMovementAsync(id, request.MovementType, request.Description, request.ToArea, request.ToResponsible, request.ToLocation, CurrentUserId(), HttpContext.RequestAborted);
            return movement is null ? NotFound() : Ok(movement);
        }

        [HttpPost("/api/assets/{id:int}/mantenimiento")]
        [PermissionAuthorize("assets.update")]
        public async Task<IActionResult> Maintenance(int id, [FromBody] MaintenanceRequest request)
        {
            var asset = await _assetService.SetMaintenanceAsync(id, request.Description, CurrentUserId(), HttpContext.RequestAborted);
            return asset is null ? NotFound() : Ok(asset);
        }

        [HttpDelete("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.delete")]
        public async Task<IActionResult> Retire(int id, [FromBody] RetirementRequest request)
        {
            try
            {
                var retired = await _assetService.RetireAsync(id, request.Reason, CurrentUserId(), HttpContext.RequestAborted);
                if (!retired.HasValue) return NotFound();
                return NoContent();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { error = exception.Message });
            }
        }

        [HttpGet("/api/assets/{assetId:int}/attachments/{attachmentId:int}/download")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> DownloadAttachment(int assetId, int attachmentId)
        {
            var attachment = await _assetService.GetAttachmentDownloadAsync(assetId, attachmentId, HttpContext.RequestAborted);
            if (attachment == null) return NotFound();
            var root = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            var safeName = Path.GetFileName(attachment.StoredFileName);
            var path = Path.Combine(root, safeName);
            if (!System.IO.File.Exists(path)) return NotFound();
            return PhysicalFile(path, attachment.ContentType, attachment.OriginalFileName, enableRangeProcessing: true);
        }

        [HttpPost("/api/assets/import")]
        [PermissionAuthorize("assets.create")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { error = "Debe proporcionar un archivo .xlsx." });
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return BadRequest(new { error = "El archivo no contiene hojas." });
            var rows = sheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();
            var parsedRows = rows.Select(row => new AssetImportRowDto(
                row.RowNumber(),
                row.Cell(1).GetString().Trim(),
                row.Cell(2).GetString().Trim(),
                row.Cell(3).GetString().Trim(),
                row.Cell(4).GetString().Trim(),
                row.Cell(5).GetString().Trim(),
                row.Cell(6).GetString().Trim(),
                row.Cell(7).GetString().Trim(),
                row.Cell(8).GetString().Trim(),
                decimal.TryParse(row.Cell(9).GetString(), out var value) ? value : 0)).ToList();

            var result = await _assetService.ImportAsync(parsedRows, HttpContext.RequestAborted);
            return Ok(new { imported = result.Imported, rejected = result.Rejected.Select(r => new { row = r.Row, code = r.Code, reason = r.Reason }) });
        }

        [HttpPost("/api/assets/import/preview")]
        [PermissionAuthorize("assets.create")]
        [RequestSizeLimit(20_000_000)]
        public IActionResult ImportPreview(IFormFile file)
        {
            if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { error = "Debe proporcionar un archivo .xlsx." });
            using var workbook = new XLWorkbook(file.OpenReadStream()); var sheet = workbook.Worksheets.FirstOrDefault(); if (sheet == null) return BadRequest(new { error = "El archivo no contiene hojas." });
            var preview = sheet.RangeUsed()?.RowsUsed().Skip(1).Take(100).Select(row => (object)new { row = row.RowNumber(), code = row.Cell(1).GetString().Trim(), name = row.Cell(2).GetString().Trim(), type = row.Cell(3).GetString().Trim(), area = row.Cell(5).GetString().Trim(), valid = !string.IsNullOrWhiteSpace(row.Cell(1).GetString()) && !string.IsNullOrWhiteSpace(row.Cell(2).GetString()) }).ToList() ?? new List<object>();
            return Ok(new { totalPreview = preview.Count, rows = preview });
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            return id > 0 ? View(id) : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10_000_000)] // 10 MB maximum request size
        public async Task<IActionResult> UploadDocument(IFormFile file, int? assetId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, error = "No file provided" });

            // Server-side validation
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
            var maxFileBytes = 5_000_000; // 5 MB

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest(new { success = false, error = "Tipo de archivo no permitido." });
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new { success = false, error = "Tipo de contenido no permitido." });
            }

            if (file.Length > maxFileBytes)
            {
                return BadRequest(new { success = false, error = "El archivo excede el tamaño máximo permitido (5 MB)." });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/{fileName}";

            if (assetId.HasValue)
            {
                var attachment = await _assetService.AttachDocumentAsync(assetId.Value, file.FileName, fileName, relativePath, file.ContentType, file.Length, CurrentUserId(), HttpContext.RequestAborted);
                if (attachment is null) return NotFound(new { error = "Bien no encontrado." });
                relativePath = attachment.RelativePath;
            }

            return Json(new { success = true, url = relativePath, name = file.FileName });
        }

        private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public sealed class AssetRequest
        {
            public string AssetCode { get; set; } = string.Empty;
            public string Barcode { get; set; } = string.Empty;
            public string? SerialNumber { get; set; }
            public string Name { get; set; } = string.Empty;
            public string AssetType { get; set; } = string.Empty;
            public string? Brand { get; set; }
            public string? Model { get; set; }
            public string PhysicalCondition { get; set; } = "Bueno";
            public string Status { get; set; } = "Activo";
            public string? Area { get; set; }
            public string? Responsible { get; set; }
            public string? Location { get; set; }
            public DateTime? AcquisitionDate { get; set; }
            public decimal AcquisitionValue { get; set; }

            public CreateAssetDto ToDto() => new(
                AssetCode, Barcode, Name, AssetType, SerialNumber, Brand, Model,
                PhysicalCondition, Status, Area, Responsible, Location, AcquisitionDate, AcquisitionValue);

            public Asset ToEntity() => new()
            {
                AssetCode = AssetCode.Trim(), Barcode = Barcode.Trim(), SerialNumber = SerialNumber?.Trim(), Name = Name.Trim(), AssetType = AssetType.Trim(), Brand = Brand?.Trim(), Model = Model?.Trim(), PhysicalCondition = PhysicalCondition, Status = Status, Area = Area, Responsible = Responsible, Location = Location, AcquisitionDate = AcquisitionDate, AcquisitionValue = AcquisitionValue
            };
        }

        public sealed class MovementRequest
        {
            public string MovementType { get; set; } = "Transferencia";
            public string Description { get; set; } = string.Empty;
            public string? ToArea { get; set; }
            public string? ToResponsible { get; set; }
            public string? ToLocation { get; set; }
        }

        public sealed class MaintenanceRequest { public string Description { get; set; } = "Mantenimiento registrado"; }
        public sealed class RetirementRequest { public string Reason { get; set; } = string.Empty; }
    }
}
