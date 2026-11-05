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
[Route("api/transferencias")]
public class TransferenciasController : ControllerBase
{
    private readonly AppDbContext _db;
    public TransferenciasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var query = _db.AssetTransfers.AsNoTracking().Include(t => t.Asset).Include(t => t.RequestedByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(t => t.Status == status);
        return Ok(await query.OrderByDescending(t => t.RequestedAt).ToListAsync());
    }

    [HttpPost]
    [PermissionAuthorize("assets.update")]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        var asset = await _db.Assets.FindAsync(request.AssetId);
        if (asset == null) return NotFound(new { error = "Bien no encontrado." });
        if (string.IsNullOrWhiteSpace(request.ToArea) || string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { error = "Área destino y motivo son obligatorios." });
        var transfer = new AssetTransfer { AssetId = asset.Id, FromArea = asset.Area ?? string.Empty, ToArea = request.ToArea.Trim(), FromResponsible = asset.Responsible, ToResponsible = request.ToResponsible, FromLocation = asset.Location, ToLocation = request.ToLocation, Reason = request.Reason.Trim(), RequestedByUserId = userId.Value };
        _db.AssetTransfers.Add(transfer);
        _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Solicitar transferencia", EntityName = nameof(AssetTransfer), Description = $"Transferencia solicitada para {asset.AssetCode}." });
        await NotifyUsers("Transferencia pendiente", $"Se solicitó transferir el bien {asset.AssetCode} a {transfer.ToArea}.", "Transferencia", "/Operaciones/Transferencias");
        await _db.SaveChangesAsync();
        return Created($"/api/transferencias/{transfer.Id}", transfer);
    }

    [HttpPatch("{id:int}/aprobar")]
    [PermissionAuthorize("transfers.approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var transfer = await _db.AssetTransfers.Include(t => t.Asset).FirstOrDefaultAsync(t => t.Id == id);
        var approverId = CurrentUserId();
        if (transfer == null) return NotFound();
        if (!approverId.HasValue) return Unauthorized();
        if (transfer.Status != "Pendiente") return BadRequest(new { error = "La transferencia ya fue procesada." });
        transfer.Status = "Aprobada"; transfer.ApprovedByUserId = approverId; transfer.ApprovedAt = DateTime.UtcNow;
        transfer.Asset.Area = transfer.ToArea; transfer.Asset.Responsible = transfer.ToResponsible; transfer.Asset.Location = transfer.ToLocation; transfer.Asset.UpdatedAt = DateTime.UtcNow;
        _db.AssetMovements.Add(new AssetMovement { AssetId = transfer.AssetId, MovementType = "Transferencia", Description = transfer.Reason, FromArea = transfer.FromArea, ToArea = transfer.ToArea, FromResponsible = transfer.FromResponsible, ToResponsible = transfer.ToResponsible, FromLocation = transfer.FromLocation, ToLocation = transfer.ToLocation, PerformedByUserId = approverId });
        _db.ActivityLogs.Add(new ActivityLog { UserId = approverId, Action = "Aprobar transferencia", EntityName = nameof(AssetTransfer), EntityId = id.ToString(), Description = $"Transferencia del bien {transfer.Asset.AssetCode} aprobada." });
        await NotifyUser(transfer.RequestedByUserId, "Transferencia aprobada", $"La transferencia del bien {transfer.Asset.AssetCode} fue aprobada.", "/Operaciones/Transferencias");
        await _db.SaveChangesAsync();
        return Ok(transfer);
    }

    [HttpPatch("{id:int}/rechazar")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectionRequest request)
    {
        var transfer = await _db.AssetTransfers.FindAsync(id);
        var userId = CurrentUserId();
        if (transfer == null) return NotFound();
        if (!userId.HasValue) return Unauthorized();
        if (transfer.Status != "Pendiente") return BadRequest(new { error = "La transferencia ya fue procesada." });
        transfer.Status = "Rechazada";
        _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Rechazar transferencia", EntityName = nameof(AssetTransfer), EntityId = id.ToString(), Description = request.Reason ?? "Transferencia rechazada." });
        await NotifyUser(transfer.RequestedByUserId, "Transferencia rechazada", request.Reason ?? "La transferencia fue rechazada.", "/Operaciones/Transferencias");
        await _db.SaveChangesAsync();
        return Ok(transfer);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private async Task NotifyUser(int userId, string title, string message, string link) { _db.Notifications.Add(new Notification { UserId = userId, Type = "Transferencia", Title = title, Message = message, Link = link }); await Task.CompletedTask; }
    private async Task NotifyUsers(string title, string message, string type, string link)
    {
        var users = await _db.Users.Where(u => u.ActiveFlag).Select(u => u.Id).ToListAsync();
        _db.Notifications.AddRange(users.Select(id => new Notification { UserId = id, Type = type, Title = title, Message = message, Link = link }));
    }
    public sealed class TransferRequest { public int AssetId { get; set; } public string ToArea { get; set; } = string.Empty; public string? ToResponsible { get; set; } public string? ToLocation { get; set; } public string Reason { get; set; } = string.Empty; }
    public sealed class RejectionRequest { public string? Reason { get; set; } }
}
