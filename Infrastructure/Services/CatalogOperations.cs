using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class CatalogOperations : ICatalogOperations
{
    private readonly AppDbContext _db;
    public CatalogOperations(AppDbContext db) => _db = db;

    private static string NormalizeUserName(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '.').ToArray();
        var normalized = new string(chars);
        return string.IsNullOrWhiteSpace(normalized) ? "responsable" : normalized;
    }

    private static string[] SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return [string.Empty, string.Empty, string.Empty];
        if (parts.Length == 1) return [parts[0], string.Empty, string.Empty];
        if (parts.Length == 2) return [parts[0], parts[1], string.Empty];
        return [parts[0], parts[1], string.Join(' ', parts.Skip(2))];
    }

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

    public async Task<object?> CreateResponsableAsync(string fullName, string userName, string email, bool active, CancellationToken cancellationToken = default)
    {
        var trimmedName = fullName.Trim();
        var normalizedUserName = NormalizeUserName(string.IsNullOrWhiteSpace(userName) ? trimmedName.Replace(' ', '.') : userName);
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(user => user.UserName == normalizedUserName, cancellationToken))
            throw new InvalidOperationException("El nombre de usuario ya existe.");

        if (await _db.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
            throw new InvalidOperationException("El correo ya está registrado.");

        var names = SplitName(trimmedName);
        var user = new User
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            Name = names[0],
            LastName = names[1],
            SecondLastName = names[2],
            PasswordHash = string.Empty,
            ActiveFlag = active,
            LastUpdate = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new { user.Id, user.UserName, user.Email, FullName = user.FullName, user.ActiveFlag };
    }

    public async Task<object?> UpdateResponsableAsync(int id, string fullName, string userName, string email, bool active, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return null;

        var trimmedName = fullName.Trim();
        var normalizedUserName = NormalizeUserName(string.IsNullOrWhiteSpace(userName) ? trimmedName.Replace(' ', '.') : userName);
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(x => x.Id != id && x.UserName == normalizedUserName, cancellationToken))
            throw new InvalidOperationException("El nombre de usuario ya existe.");

        if (await _db.Users.AnyAsync(x => x.Id != id && x.Email == normalizedEmail, cancellationToken))
            throw new InvalidOperationException("El correo ya está registrado.");

        var names = SplitName(trimmedName);
        user.UserName = normalizedUserName;
        user.Email = normalizedEmail;
        user.Name = names[0];
        user.LastName = names[1];
        user.SecondLastName = names[2];
        user.ActiveFlag = active;
        user.LastUpdate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new { user.Id, user.UserName, user.Email, FullName = user.FullName, user.ActiveFlag };
    }
}
