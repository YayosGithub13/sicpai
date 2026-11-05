using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogosController(AppDbContext db) => _db = db;

    [HttpGet("areas")]
    public async Task<IActionResult> Areas([FromQuery] bool includeInactive = false)
    {
        var query = _db.InstitutionalAreas.AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(a => a.Active);
        return Ok(await query.OrderBy(a => a.Name).ToListAsync());
    }

    [HttpPost("areas")]
    public async Task<IActionResult> CreateArea([FromBody] AreaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "El código y nombre del área son obligatorios." });
        if (await _db.InstitutionalAreas.AnyAsync(a => a.Code == request.Code))
            return Conflict(new { error = "El código del área ya existe." });
        if (request.ParentAreaId.HasValue && !await _db.InstitutionalAreas.AnyAsync(a => a.Id == request.ParentAreaId))
            return BadRequest(new { error = "El área superior no existe." });

        var area = new InstitutionalArea { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description?.Trim(), ParentAreaId = request.ParentAreaId };
        _db.InstitutionalAreas.Add(area);
        await WriteLog("Crear", nameof(InstitutionalArea), area.Code, $"Área {area.Name} creada.");
        await _db.SaveChangesAsync();
        return Created($"/api/catalogos/areas/{area.Id}", area);
    }

    [HttpPut("areas/{id:int}")]
    public async Task<IActionResult> UpdateArea(int id, [FromBody] AreaRequest request)
    {
        var area = await _db.InstitutionalAreas.FindAsync(id);
        if (area == null) return NotFound();
        if (await _db.InstitutionalAreas.AnyAsync(a => a.Id != id && a.Code == request.Code)) return Conflict(new { error = "El código del área ya existe." });
        area.Code = request.Code.Trim(); area.Name = request.Name.Trim(); area.Description = request.Description?.Trim(); area.ParentAreaId = request.ParentAreaId; area.Active = request.Active; area.UpdatedAt = DateTime.UtcNow;
        await WriteLog("Actualizar", nameof(InstitutionalArea), area.Id.ToString(), $"Área {area.Name} actualizada.");
        await _db.SaveChangesAsync();
        return Ok(area);
    }

    [HttpGet("ubicaciones")]
    public async Task<IActionResult> Locations([FromQuery] bool includeInactive = false)
    {
        var query = _db.PhysicalLocations.AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(l => l.Active);
        return Ok(await query.OrderBy(l => l.Site).ThenBy(l => l.Name).ToListAsync());
    }

    [HttpPost("ubicaciones")]
    public async Task<IActionResult> CreateLocation([FromBody] LocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Site) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Código, sede y nombre son obligatorios." });
        if (await _db.PhysicalLocations.AnyAsync(l => l.Code == request.Code)) return Conflict(new { error = "El código de ubicación ya existe." });
        var location = new PhysicalLocation { Code = request.Code.Trim(), Site = request.Site.Trim(), Building = request.Building?.Trim(), Floor = request.Floor?.Trim(), Name = request.Name.Trim(), Description = request.Description?.Trim() };
        _db.PhysicalLocations.Add(location);
        await WriteLog("Crear", nameof(PhysicalLocation), location.Code, $"Ubicación {location.Name} creada.");
        await _db.SaveChangesAsync();
        return Created($"/api/catalogos/ubicaciones/{location.Id}", location);
    }

    [HttpPut("ubicaciones/{id:int}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationRequest request)
    {
        var location = await _db.PhysicalLocations.FindAsync(id);
        if (location == null) return NotFound();
        if (await _db.PhysicalLocations.AnyAsync(l => l.Id != id && l.Code == request.Code)) return Conflict(new { error = "El código de ubicación ya existe." });
        location.Code = request.Code.Trim(); location.Site = request.Site.Trim(); location.Building = request.Building?.Trim(); location.Floor = request.Floor?.Trim(); location.Name = request.Name.Trim(); location.Description = request.Description?.Trim(); location.Active = request.Active; location.UpdatedAt = DateTime.UtcNow;
        await WriteLog("Actualizar", nameof(PhysicalLocation), location.Id.ToString(), $"Ubicación {location.Name} actualizada.");
        await _db.SaveChangesAsync();
        return Ok(location);
    }

    private async Task WriteLog(string action, string entityName, string entityId, string description)
    {
        _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = action, EntityName = entityName, EntityId = entityId, Description = description, IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
        await Task.CompletedTask;
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public sealed class AreaRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentAreaId { get; set; }
        public bool Active { get; set; } = true;
    }

    public sealed class LocationRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
