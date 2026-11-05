using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using System.Security.Claims;
using ClosedXML.Excel;
using SICPAI.Security;


namespace SICPAI.Controllers
{
    [Authorize]
    public class PatrimonioController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public PatrimonioController(IWebHostEnvironment env, AppDbContext db)
        {
            _env = env;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/assets")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? area)
        {
            var query = _db.Assets.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.AssetCode.Contains(search) || a.Name.Contains(search) || (a.SerialNumber ?? "").Contains(search));
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
            if (!string.IsNullOrWhiteSpace(area)) query = query.Where(a => a.Area == area);

            var assets = await query.OrderByDescending(a => a.UpdatedAt).ToListAsync();
            return Ok(assets);
        }

        [HttpGet("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> Get(int id)
        {
            var asset = await _db.Assets.AsNoTracking()
                .Include(a => a.Movements.OrderByDescending(m => m.OccurredAt))
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);
            return asset == null ? NotFound() : Ok(asset);
        }

        [HttpGet("/api/assets/scan/{code}")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> Scan(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest(new { error = "Código requerido." });
            var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Barcode == code || a.AssetCode == code);
            return asset == null ? NotFound(new { error = "No se encontró un bien con ese código." }) : Ok(asset);
        }

        [HttpPost("/api/assets")]
        [PermissionAuthorize("assets.create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsset([FromBody] AssetRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.AssetCode) || string.IsNullOrWhiteSpace(request.Name))
                return ValidationProblem(ModelState);
            if (await _db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode))
                return Conflict(new { error = "El código patrimonial ya existe." });

            var asset = request.ToEntity();
            _db.Assets.Add(asset);
            asset.Movements.Add(new AssetMovement { MovementType = "Alta", Description = "Alta inicial del bien", ToArea = asset.Area, ToResponsible = asset.Responsible, ToLocation = asset.Location, PerformedByUserId = CurrentUserId() });
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Crear", EntityName = nameof(Asset), EntityId = asset.AssetCode, Description = $"Bien {asset.Name} registrado." });
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = asset.Id }, asset);
        }

        [HttpPut("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetRequest request)
        {
            var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null) return NotFound();
            if (await _db.Assets.AnyAsync(a => a.Id != id && a.AssetCode == request.AssetCode))
                return Conflict(new { error = "El código patrimonial ya existe." });

            var changes = new List<string>();
            if (asset.Area != request.Area || asset.Responsible != request.Responsible || asset.Location != request.Location)
                changes.Add("Se actualizó la asignación del bien.");
            if (asset.Status != request.Status) changes.Add($"Cambio de estado: {asset.Status} → {request.Status}.");
            asset.UpdateFrom(request);
            asset.UpdatedAt = DateTime.UtcNow;
            foreach (var change in changes)
                asset.Movements.Add(new AssetMovement { MovementType = "Actualización", Description = change, PerformedByUserId = CurrentUserId() });
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Actualizar", EntityName = nameof(Asset), EntityId = id.ToString(), Description = $"Bien {asset.AssetCode} actualizado." });
            await _db.SaveChangesAsync();
            return Ok(asset);
        }

        [HttpPost("/api/assets/{id:int}/movements")]
        [PermissionAuthorize("assets.update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovement(int id, [FromBody] MovementRequest request)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            var movement = new AssetMovement { AssetId = id, MovementType = request.MovementType, Description = request.Description, FromArea = asset.Area, ToArea = request.ToArea, FromResponsible = asset.Responsible, ToResponsible = request.ToResponsible, FromLocation = asset.Location, ToLocation = request.ToLocation, PerformedByUserId = CurrentUserId() };
            asset.Area = request.ToArea ?? asset.Area;
            asset.Responsible = request.ToResponsible ?? asset.Responsible;
            asset.Location = request.ToLocation ?? asset.Location;
            asset.UpdatedAt = DateTime.UtcNow;
            _db.AssetMovements.Add(movement);
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Movimiento", EntityName = nameof(Asset), EntityId = id.ToString(), Description = movement.Description });
            await _db.SaveChangesAsync();
            return Ok(movement);
        }

        [HttpPost("/api/assets/{id:int}/mantenimiento")]
        [PermissionAuthorize("assets.update")]
        public async Task<IActionResult> Maintenance(int id, [FromBody] MaintenanceRequest request)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            asset.Status = "En reparación"; asset.UpdatedAt = DateTime.UtcNow;
            _db.AssetMovements.Add(new AssetMovement { AssetId = id, MovementType = "Mantenimiento", Description = request.Description, PerformedByUserId = CurrentUserId() });
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Mantenimiento", EntityName = nameof(Asset), EntityId = id.ToString(), Description = request.Description });
            await _db.SaveChangesAsync();
            return Ok(asset);
        }

        [HttpDelete("/api/assets/{id:int}")]
        [PermissionAuthorize("assets.delete")]
        public async Task<IActionResult> Retire(int id, [FromBody] RetirementRequest request)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { error = "El motivo de baja es obligatorio." });
            asset.Status = "Baja definitiva"; asset.UpdatedAt = DateTime.UtcNow;
            _db.AssetMovements.Add(new AssetMovement { AssetId = id, MovementType = "Baja", Description = request.Reason, PerformedByUserId = CurrentUserId() });
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Baja definitiva", EntityName = nameof(Asset), EntityId = id.ToString(), Description = request.Reason });
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("/api/assets/{assetId:int}/attachments/{attachmentId:int}/download")]
        [PermissionAuthorize("assets.read")]
        public async Task<IActionResult> DownloadAttachment(int assetId, int attachmentId)
        {
            var attachment = await _db.AssetAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.AssetId == assetId);
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
            var imported = 0; var rejected = new List<object>();
            await using var transaction = await _db.Database.BeginTransactionAsync();
            foreach (var row in rows)
            {
                var code = row.Cell(1).GetString().Trim(); var name = row.Cell(2).GetString().Trim();
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || await _db.Assets.AnyAsync(a => a.AssetCode == code)) { rejected.Add(new { row = row.RowNumber(), code, reason = "Código o nombre inválido, o código duplicado." }); continue; }
                _db.Assets.Add(new Asset { AssetCode = code, Name = name, AssetType = row.Cell(3).GetString().Trim(), SerialNumber = row.Cell(4).GetString().Trim(), Area = row.Cell(5).GetString().Trim(), Responsible = row.Cell(6).GetString().Trim(), Location = row.Cell(7).GetString().Trim(), Status = string.IsNullOrWhiteSpace(row.Cell(8).GetString()) ? "Activo" : row.Cell(8).GetString().Trim(), AcquisitionValue = decimal.TryParse(row.Cell(9).GetString(), out var value) ? value : 0 });
                imported++;
            }
            await _db.SaveChangesAsync(); await transaction.CommitAsync();
            return Ok(new { imported, rejected });
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

        public IActionResult Detail()
        {
            return View();
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

            if (assetId.HasValue && !await _db.Assets.AnyAsync(a => a.Id == assetId.Value)) return NotFound(new { error = "Bien no encontrado." });
            var uploadsRoot = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
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
                var attachment = new AssetAttachment { AssetId = assetId.Value, OriginalFileName = file.FileName, StoredFileName = fileName, RelativePath = relativePath, ContentType = file.ContentType, Size = file.Length, UploadedByUserId = CurrentUserId() };
                _db.AssetAttachments.Add(attachment);
                await _db.SaveChangesAsync();
                attachment.RelativePath = relativePath = $"/api/assets/{assetId.Value}/attachments/{attachment.Id}/download";
                await _db.SaveChangesAsync();
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

internal static class AssetExtensions
{
    public static void UpdateFrom(this Asset asset, SICPAI.Controllers.PatrimonioController.AssetRequest request)
    {
        asset.AssetCode = request.AssetCode.Trim(); asset.Barcode = request.Barcode.Trim(); asset.SerialNumber = request.SerialNumber?.Trim(); asset.Name = request.Name.Trim(); asset.AssetType = request.AssetType.Trim(); asset.Brand = request.Brand?.Trim(); asset.Model = request.Model?.Trim(); asset.PhysicalCondition = request.PhysicalCondition; asset.Status = request.Status; asset.Area = request.Area; asset.Responsible = request.Responsible; asset.Location = request.Location; asset.AcquisitionDate = request.AcquisitionDate; asset.AcquisitionValue = request.AcquisitionValue;
    }
}
