using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();
    public AdminService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<object>> ListUsersAsync(CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking().Include(u => u.UserRoles).ThenInclude(ur => ur.RoleCatalog).OrderBy(u => u.Name)
            .Select(u => (object)new { u.Id, u.UserName, u.Email, u.Name, u.LastName, u.ActiveFlag, Roles = u.UserRoles.Where(ur => ur.ActiveFlag).Select(ur => ur.RoleCatalog.Name) }).ToListAsync(cancellationToken);

    public async Task<User?> CreateUserAsync(string userName, string email, string name, string lastName, string password, int? roleId, int? actorId, CancellationToken cancellationToken = default)
    {
        if (await _db.Users.AnyAsync(u => u.UserName == userName || u.Email == email, cancellationToken)) throw new InvalidOperationException("El usuario o correo ya existe.");
        var user = new User(userName.Trim(), email.Trim(), string.Empty) { Name = name.Trim(), LastName = lastName.Trim() };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _db.Users.Add(user); await _db.SaveChangesAsync(cancellationToken);
        if (roleId.HasValue && await _db.RoleCatalogs.AnyAsync(r => r.Id == roleId, cancellationToken)) _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleCatalogId = roleId.Value, ActiveFlag = true, LastUpdate = DateTime.UtcNow });
        _db.ActivityLogs.Add(new ActivityLog { UserId = actorId, Action = "Crear", EntityName = nameof(User), EntityId = user.Id.ToString(), Description = $"Usuario {user.UserName} creado." });
        await _db.SaveChangesAsync(cancellationToken); return user;
    }

    public async Task<User?> UpdateUserStatusAsync(int id, bool active, int? actorId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FindAsync([id], cancellationToken); if (user is null) return null;
        user.ActiveFlag = active; user.LastUpdate = DateTime.UtcNow;
        _db.ActivityLogs.Add(new ActivityLog { UserId = actorId, Action = "Actualizar estado", EntityName = nameof(User), EntityId = id.ToString(), Description = $"Usuario {user.UserName}: {(active ? "activado" : "desactivado")}." });
        await _db.SaveChangesAsync(cancellationToken); return user;
    }

    public async Task<IReadOnlyList<RoleCatalog>> ListRolesAsync(CancellationToken cancellationToken = default) => await _db.RoleCatalogs.AsNoTracking().Include(r => r.RolePermissions).ThenInclude(rp => rp.PermissionCatalog).OrderBy(r => r.Name).ToListAsync(cancellationToken);
    public async Task<RoleCatalog?> CreateRoleAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        if (await _db.RoleCatalogs.AnyAsync(r => r.Name == name, cancellationToken)) throw new InvalidOperationException("El rol ya existe.");
        var role = new RoleCatalog { Name = name.Trim(), Description = description?.Trim(), ActiveFlag = true, LastUpdate = DateTime.UtcNow }; _db.RoleCatalogs.Add(role); await _db.SaveChangesAsync(cancellationToken); return role;
    }
    public async Task<RoleCatalog?> UpdateRoleAsync(int id, string name, string? description, bool active, CancellationToken cancellationToken = default)
    {
        var role = await _db.RoleCatalogs.FindAsync([id], cancellationToken); if (role is null) return null;
        role.Name = name.Trim(); role.Description = description?.Trim(); role.ActiveFlag = active; role.LastUpdate = DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken); return role;
    }
    public async Task<IReadOnlyList<PermissionCatalog>> ListPermissionsAsync(CancellationToken cancellationToken = default) => await _db.PermissionCatalogs.AsNoTracking().Where(p => p.Active).OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<int>?> SetRolePermissionsAsync(int roleId, IReadOnlyCollection<int> permissionIds, int? actorId, CancellationToken cancellationToken = default)
    {
        if (!await _db.RoleCatalogs.AnyAsync(r => r.Id == roleId, cancellationToken)) return null;
        var validIds = await _db.PermissionCatalogs.Where(p => permissionIds.Contains(p.Id)).Select(p => p.Id).ToListAsync(cancellationToken);
        var current = await _db.RolePermissions.Where(rp => rp.RoleCatalogId == roleId).ToListAsync(cancellationToken); _db.RolePermissions.RemoveRange(current); _db.RolePermissions.AddRange(validIds.Select(id => new RolePermission { RoleCatalogId = roleId, PermissionCatalogId = id }));
        _db.ActivityLogs.Add(new ActivityLog { UserId = actorId, Action = "Actualizar permisos", EntityName = nameof(RoleCatalog), EntityId = roleId.ToString(), Description = $"Se asignaron {validIds.Count} permisos." }); await _db.SaveChangesAsync(cancellationToken); return validIds;
    }
}
