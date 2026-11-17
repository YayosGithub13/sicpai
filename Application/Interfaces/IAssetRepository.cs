using Domain.Entities;

namespace Application.Interfaces;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> ListAsync(string? search, string? status, string? area, CancellationToken cancellationToken = default);
    Task<Asset?> GetAsync(int id, bool includeHistory = false, CancellationToken cancellationToken = default);
    Task<Asset?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssetMovement>> ListMovementsAsync(string? search, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<AssetAttachment?> GetAttachmentAsync(int assetId, int attachmentId, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(string code, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task AddAttachmentAsync(AssetAttachment attachment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}