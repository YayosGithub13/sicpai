using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SICPAI.Security;
using Application.Interfaces;
using Domain.Entities;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/auditorias")]
public class AuditoriasController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditOperations _auditOperations;
    public AuditoriasController(IAuditService auditService, IAuditOperations auditOperations)
    {
        _auditService = auditService;
        _auditOperations = auditOperations;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status) => Ok(await _auditService.ListAsync(status, HttpContext.RequestAborted));

    [HttpGet("{id:int}/bienes")]
    public async Task<IActionResult> Assets(int id)
    {
        var assets = await _auditOperations.GetAssetsAsync(id, HttpContext.RequestAborted);
        return assets == null ? NotFound() : Ok(assets);
    }

    [HttpPost("{id:int}/bienes")]
    public async Task<IActionResult> AddAsset(int id, [FromBody] AssetRequest request)
    {
        try
        {
            var result = await _auditOperations.AddAssetAsync(id, request.AssetId, HttpContext.RequestAborted);
            return result == null ? NotFound() : Created($"/api/auditorias/{id}/bienes/{request.AssetId}", result);
        }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpPatch("{id:int}/bienes/{assetId:int}")]
    public async Task<IActionResult> VerifyAsset(int id, int assetId, [FromBody] VerificationRequest request)
    {
        var valid = new[] { "Encontrado", "Faltante", "Dañado", "Pendiente" };
        if (!valid.Contains(request.Result)) return BadRequest(new { error = "Resultado no válido." });
        var result = await _auditOperations.VerifyAssetAsync(id, assetId, request.Result, request.Notes, CurrentUserId(), HttpContext.RequestAborted);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Create([FromBody] AuditRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.ScheduledDate == default) return BadRequest(new { error = "El nombre y la fecha programada son obligatorios." });
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        var audit = await _auditOperations.CreateAsync(request.Name, request.Description, request.ScheduledDate, request.AreaId, userId.Value, HttpContext.RequestAborted);
        return audit == null ? BadRequest(new { error = "El área no existe." }) : Created($"/api/auditorias/{audit.Id}", audit);
    }

    [HttpPatch("{id:int}/estado")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusRequest request)
    {
        var allowed = new[] { "Programada", "En proceso", "Completada", "Cancelada" };
        if (!allowed.Contains(request.Status)) return BadRequest(new { error = "Estado de auditoría no válido." });
        var audit = await _auditOperations.UpdateStatusAsync(id, request.Status, CurrentUserId(), HttpContext.RequestAborted);
        return audit == null ? NotFound() : Ok(audit);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public sealed class AuditRequest { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public DateTime ScheduledDate { get; set; } public int? AreaId { get; set; } }
    public sealed class StatusRequest { public string Status { get; set; } = string.Empty; }
    public sealed class AssetRequest { public int AssetId { get; set; } }
    public sealed class VerificationRequest { public string Result { get; set; } = "Encontrado"; public string? Notes { get; set; } }
}
