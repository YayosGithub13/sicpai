using Domain.Entities;

namespace Application.Interfaces;

public interface IInventoryOperations
{
    Task<InventorySession?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<InventorySession?> StartAsync(string name, string? area, int userId, CancellationToken cancellationToken = default);
    Task<InventoryCount?> CountAsync(int sessionId, int assetId, string result, string? notes, int? userId, CancellationToken cancellationToken = default);
    Task<InventorySession?> CloseAsync(int id, int? userId, CancellationToken cancellationToken = default);
}

public interface IAuditOperations
{
    Task<IReadOnlyList<AuditAssetResult>?> GetAssetsAsync(int id, CancellationToken cancellationToken = default);
    Task<AuditAssetResult?> AddAssetAsync(int id, int assetId, CancellationToken cancellationToken = default);
    Task<AuditAssetResult?> VerifyAssetAsync(int id, int assetId, string result, string? notes, int? userId, CancellationToken cancellationToken = default);
    Task<AuditRecord?> CreateAsync(string name, string? description, DateTime scheduledDate, int? areaId, int userId, CancellationToken cancellationToken = default);
    Task<AuditRecord?> UpdateStatusAsync(int id, string status, int? userId, CancellationToken cancellationToken = default);
}

public interface ICatalogOperations
{
    Task<InstitutionalArea?> CreateAreaAsync(InstitutionalArea area, CancellationToken cancellationToken = default);
    Task<InstitutionalArea?> UpdateAreaAsync(int id, InstitutionalArea request, CancellationToken cancellationToken = default);
    Task<PhysicalLocation?> CreateLocationAsync(PhysicalLocation location, CancellationToken cancellationToken = default);
    Task<PhysicalLocation?> UpdateLocationAsync(int id, PhysicalLocation request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetResponsablesAsync(bool includeInactive, CancellationToken cancellationToken = default);
}
