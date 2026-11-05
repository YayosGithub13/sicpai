using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SICPAI.Security;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/auditorias")]
public class AuditoriasController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuditoriasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var query = _db.AuditRecords.AsNoTracking().Include(a => a.Area).Include(a => a.CreatedByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        return Ok(await query.OrderByDescending(a => a.ScheduledDate).ToListAsync());
    }

    [HttpGet("{id:int}/bienes")]
    public async Task<IActionResult> Assets(int id)
    {
        var audit = await _db.AuditRecords.AsNoTracking().Include(a => a.AssetResults).ThenInclude(r => r.Asset).FirstOrDefaultAsync(a => a.Id == id);
        return audit == null ? NotFound() : Ok(audit.AssetResults);
    }

    [HttpPost("{id:int}/bienes")]
    public async Task<IActionResult> AddAsset(int id, [FromBody] AssetRequest request)
    {
        if (!await _db.AuditRecords.AnyAsync(a => a.Id == id) || !await _db.Assets.AnyAsync(a => a.Id == request.AssetId)) return NotFound();
        if (await _db.AuditAssetResults.AnyAsync(r => r.AuditRecordId == id && r.AssetId == request.AssetId)) return Conflict(new { error = "El bien ya está incluido en la auditoría." });
        var result = new AuditAssetResult { AuditRecordId = id, AssetId = request.AssetId };
        _db.AuditAssetResults.Add(result);
        await _db.SaveChangesAsync();
        return Created($"/api/auditorias/{id}/bienes/{request.AssetId}", result);
    }

    [HttpPatch("{id:int}/bienes/{assetId:int}")]
    public async Task<IActionResult> VerifyAsset(int id, int assetId, [FromBody] VerificationRequest request)
    {
        var result = await _db.AuditAssetResults.FirstOrDefaultAsync(r => r.AuditRecordId == id && r.AssetId == assetId);
        if (result == null) return NotFound();
        var valid = new[] { "Encontrado", "Faltante", "Dañado", "Pendiente" };
        if (!valid.Contains(request.Result)) return BadRequest(new { error = "Resultado no válido." });
        result.Result = request.Result; result.Notes = request.Notes; result.VerifiedAt = DateTime.UtcNow; result.VerifiedByUserId = CurrentUserId();
        await _db.SaveChangesAsync();
        return Ok(result);
    }

    [HttpPost]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Create([FromBody] AuditRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.ScheduledDate == default)
            return BadRequest(new { error = "El nombre y la fecha programada son obligatorios." });
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        if (request.AreaId.HasValue && !await _db.InstitutionalAreas.AnyAsync(a => a.Id == request.AreaId)) return BadRequest(new { error = "El área no existe." });
        var audit = new AuditRecord { Name = request.Name.Trim(), Description = request.Description?.Trim(), ScheduledDate = request.ScheduledDate, AreaId = request.AreaId, CreatedByUserId = userId.Value, Status = "Programada" };
        _db.AuditRecords.Add(audit);
        _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Crear", EntityName = nameof(AuditRecord), Description = $"Auditoría {audit.Name} programada." });
        var users = await _db.Users.Where(u => u.ActiveFlag).Select(u => u.Id).ToListAsync();
        _db.Notifications.AddRange(users.Select(id => new Notification { UserId = id, Type = "Auditoría", Title = "Nueva auditoría programada", Message = audit.Name, Link = "/Operaciones/Auditorias" }));
        await _db.SaveChangesAsync();
        return Created($"/api/auditorias/{audit.Id}", audit);
    }

    [HttpPatch("{id:int}/estado")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusRequest request)
    {
        var audit = await _db.AuditRecords.FindAsync(id);
        if (audit == null) return NotFound();
        var allowed = new[] { "Programada", "En proceso", "Completada", "Cancelada" };
        if (!allowed.Contains(request.Status)) return BadRequest(new { error = "Estado de auditoría no válido." });
        audit.Status = request.Status;
        audit.ClosedDate = request.Status is "Completada" or "Cancelada" ? DateTime.UtcNow : null;
        _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Actualizar estado", EntityName = nameof(AuditRecord), EntityId = id.ToString(), Description = $"Estado cambiado a {audit.Status}." });
        await _db.SaveChangesAsync();
        return Ok(audit);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public sealed class AuditRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int? AreaId { get; set; }
    }

    public sealed class StatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public sealed class AssetRequest { public int AssetId { get; set; } }
    public sealed class VerificationRequest { public string Result { get; set; } = "Encontrado"; public string? Notes { get; set; } }
}
