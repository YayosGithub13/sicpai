using Domain.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITransferService
{
    Task<IReadOnlyList<AssetTransfer>> ListAsync(string? status, CancellationToken cancellationToken = default);
    Task<AssetTransfer?> CreateAsync(CreateTransferDto request, int userId, CancellationToken cancellationToken = default);
    Task<AssetTransfer?> ApproveAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<AssetTransfer?> RejectAsync(int id, int userId, string? reason, CancellationToken cancellationToken = default);
}

public interface IInventoryService
{
    Task<IReadOnlyList<InventorySession>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task<IReadOnlyList<AuditRecord>> ListAsync(string? status, CancellationToken cancellationToken = default);
}

public interface ICatalogService
{
    Task<IReadOnlyList<InstitutionalArea>> GetAreasAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhysicalLocation>> GetLocationsAsync(bool includeInactive, CancellationToken cancellationToken = default);
}