using Domain.Entities;

namespace Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<object>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<User?> CreateUserAsync(string userName, string email, string name, string lastName, string password, int? roleId, int? actorId, CancellationToken cancellationToken = default);
    Task<User?> UpdateUserStatusAsync(int id, bool active, int? actorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleCatalog>> ListRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleCatalog?> CreateRoleAsync(string name, string? description, CancellationToken cancellationToken = default);
    Task<RoleCatalog?> UpdateRoleAsync(int id, string name, string? description, bool active, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionCatalog>> ListPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>?> SetRolePermissionsAsync(int roleId, IReadOnlyCollection<int> permissionIds, int? actorId, CancellationToken cancellationToken = default);
}
