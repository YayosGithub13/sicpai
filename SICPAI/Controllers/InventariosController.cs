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
[Route("api/inventarios")]
public class InventariosController : ControllerBase
{
    private readonly AppDbContext _db;
    public InventariosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _db.InventorySessions.AsNoTracking().Include(i => i.CreatedByUser).OrderByDescending(i => i.StartedAt).ToListAsync());

    [HttpPost]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Start([FromBody] StartRequest request)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue || string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Nombre y usuario son obligatorios." });
        var session = new InventorySession { Name = request.Name.Trim(), CreatedByUserId = userId.Value };
        var assets = await _db.Assets.Where(a => string.IsNullOrWhiteSpace(request.Area) || a.Area == request.Area).ToListAsync();
        session.Counts = assets.Select(a => new InventoryCount { AssetId = a.Id }).ToList();
        _db.InventorySessions.Add(session);
        await _db.SaveChangesAsync();
        return Created($"/api/inventarios/{session.Id}", session);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var session = await _db.InventorySessions.AsNoTracking().Include(i => i.Counts).ThenInclude(c => c.Asset).FirstOrDefaultAsync(i => i.Id == id);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpPatch("{id:int}/conteo/{assetId:int}")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Count(int id, int assetId, [FromBody] CountRequest request)
    {
        var count = await _db.InventoryCounts.FirstOrDefaultAsync(c => c.InventorySessionId == id && c.AssetId == assetId);
        if (count == null) return NotFound();
        var valid = new[] { "Encontrado", "Faltante", "Dañado", "Pendiente" };
        if (!valid.Contains(request.Result)) return BadRequest(new { error = "Resultado de conteo no válido." });
        count.Result = request.Result; count.Notes = request.Notes; count.CountedAt = DateTime.UtcNow; count.CountedByUserId = CurrentUserId();
        await _db.SaveChangesAsync();
        return Ok(count);
    }

    [HttpPatch("{id:int}/cerrar")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Close(int id)
    {
        var session = await _db.InventorySessions.FindAsync(id);
        if (session == null) return NotFound();
        session.Status = "Cerrado"; session.ClosedAt = DateTime.UtcNow;
        var differences = await _db.InventoryCounts.Where(c => c.InventorySessionId == id && c.Result != "Encontrado").Include(c => c.Asset).ToListAsync();
        foreach (var difference in differences)
        {
            if (difference.Result == "Dañado") difference.Asset.Status = "Dañado";
            if (difference.Result == "Faltante") difference.Asset.Status = "Extraviado";
            _db.Notifications.Add(new Notification { UserId = session.CreatedByUserId, Type = "Inventario", Title = $"Bien {difference.Result.ToLowerInvariant()}", Message = $"{difference.Asset.AssetCode}: {difference.Asset.Name}", Link = "/Operaciones/Auditorias" });
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = "Conciliación", EntityName = nameof(InventoryCount), EntityId = difference.Id.ToString(), Description = $"Bien marcado como {difference.Result}." });
        }
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public sealed class StartRequest { public string Name { get; set; } = string.Empty; public string? Area { get; set; } }
    public sealed class CountRequest { public string Result { get; set; } = "Encontrado"; public string? Notes { get; set; } }
}
