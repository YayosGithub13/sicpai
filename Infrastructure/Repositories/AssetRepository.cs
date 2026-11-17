using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _dbContext;

    public AssetRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Asset>> ListAsync(string? search, string? status, string? area, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.AssetCode.Contains(search) || a.Name.Contains(search) || a.SerialNumber.Contains(search) || a.Barcode.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(area)) query = query.Where(a => a.Area == area);
        return await query.OrderByDescending(a => a.UpdatedAt).ToListAsync(cancellationToken);
    }

    public Task<Asset?> GetAsync(int id, bool includeHistory = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Asset> query = _dbContext.Assets;
        if (includeHistory)
        {
            query = query.Include(a => a.Movements).ThenInclude(m => m.PerformedByUser);
        }
        return query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public Task<Asset?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.AssetCode == code || a.Barcode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<AssetMovement>> ListMovementsAsync(string? search, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AssetMovements.AsNoTracking()
            .Include(m => m.Asset)
            .Include(m => m.PerformedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Asset.AssetCode.Contains(search) || m.Asset.Name.Contains(search));
        if (!string.IsNullOrWhiteSpace(type) && type != "Todos")
            query = query.Where(m => m.MovementType == type);
        if (from.HasValue)
            query = query.Where(m => m.OccurredAt >= from.Value.ToUniversalTime());
        if (to.HasValue)
            query = query.Where(m => m.OccurredAt < to.Value.ToUniversalTime().Date.AddDays(1));

        return await query.OrderByDescending(m => m.OccurredAt).ToListAsync(cancellationToken);
    }

    public Task<AssetAttachment?> GetAttachmentAsync(int assetId, int attachmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AssetAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.AssetId == assetId, cancellationToken);
    }

    public Task<bool> ExistsCodeAsync(string code, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Assets.AnyAsync(a => (a.AssetCode == code || a.Barcode == code) && (!excludedId.HasValue || a.Id != excludedId.Value), cancellationToken);
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        await _dbContext.Assets.AddAsync(asset, cancellationToken);
    }

    public async Task AddAttachmentAsync(AssetAttachment attachment, CancellationToken cancellationToken = default)
    {
        await _dbContext.AssetAttachments.AddAsync(attachment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}