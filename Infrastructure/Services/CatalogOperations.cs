using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class CatalogOperations : ICatalogOperations
{
    private readonly AppDbContext _db;
    public CatalogOperations(AppDbContext db) => _db = db;
    public async Task<InstitutionalArea?> CreateAreaAsync(InstitutionalArea area, CancellationToken cancellationToken = default)
    {
        if (await _db.InstitutionalAreas.AnyAsync(a => a.Code == area.Code, cancellationToken)) throw new InvalidOperationException("El código del área ya existe.");
        if (area.ParentAreaId.HasValue && !await _db.InstitutionalAreas.AnyAsync(a => a.Id == area.ParentAreaId, cancellationToken)) throw new ArgumentException("El área superior no existe.");
        _db.InstitutionalAreas.Add(area); await _db.SaveChangesAsync(cancellationToken); return area;
    }
    public async Task<InstitutionalArea?> UpdateAreaAsync(int id, InstitutionalArea request, CancellationToken cancellationToken = default)
    {
        var area = await _db.InstitutionalAreas.FindAsync([id], cancellationToken); if (area is null) return null;
        if (await _db.InstitutionalAreas.AnyAsync(a => a.Id != id && a.Code == request.Code, cancellationToken)) throw new InvalidOperationException("El código del área ya existe.");
        area.Code = request.Code.Trim(); area.Name = request.Name.Trim(); area.Description = request.Description?.Trim(); area.ParentAreaId = request.ParentAreaId; area.Active = request.Active; area.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken); return area;
    }
    public async Task<PhysicalLocation?> CreateLocationAsync(PhysicalLocation location, CancellationToken cancellationToken = default)
    {
        if (await _db.PhysicalLocations.AnyAsync(l => l.Code == location.Code, cancellationToken)) throw new InvalidOperationException("El código de ubicación ya existe.");
        _db.PhysicalLocations.Add(location); await _db.SaveChangesAsync(cancellationToken); return location;
    }
    public async Task<PhysicalLocation?> UpdateLocationAsync(int id, PhysicalLocation request, CancellationToken cancellationToken = default)
    {
        var location = await _db.PhysicalLocations.FindAsync([id], cancellationToken); if (location is null) return null;
        if (await _db.PhysicalLocations.AnyAsync(l => l.Id != id && l.Code == request.Code, cancellationToken)) throw new InvalidOperationException("El código de ubicación ya existe.");
        location.Code = request.Code.Trim(); location.Site = request.Site.Trim(); location.Building = request.Building?.Trim(); location.Floor = request.Floor?.Trim(); location.Name = request.Name.Trim(); location.Description = request.Description?.Trim(); location.Active = request.Active; location.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken); return location;
    }
    public async Task<IReadOnlyList<object>> GetResponsablesAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking().Where(u => includeInactive || u.ActiveFlag).OrderBy(u => u.Name).ThenBy(u => u.LastName).Select(u => (object)new { u.Id, u.UserName, u.Email, u.FullName, u.ActiveFlag, Roles = u.UserRoles.Where(ur => ur.ActiveFlag).Select(ur => ur.RoleCatalog.Name), AssetCount = _db.Assets.Count(a => a.Responsible == u.FullName || a.Responsible == u.UserName), AssetValue = _db.Assets.Where(a => a.Responsible == u.FullName || a.Responsible == u.UserName).Select(a => (decimal?)a.AcquisitionValue).Sum() ?? 0 }).ToListAsync(cancellationToken);
}
