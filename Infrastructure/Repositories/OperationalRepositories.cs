using Application.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TransferService : ITransferService
{
    private readonly AppDbContext _dbContext;
    public TransferService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<AssetTransfer>> ListAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AssetTransfers.AsNoTracking().Include(t => t.Asset).Include(t => t.RequestedByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(t => t.Status == status);
        return await query.OrderByDescending(t => t.RequestedAt).ToListAsync(cancellationToken);
    }

    public async Task<AssetTransfer?> CreateAsync(CreateTransferDto request, int userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ToArea) || string.IsNullOrWhiteSpace(request.Reason)) return null;
        var asset = await _dbContext.Assets.FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
        if (asset is null) return null;
        var transfer = new AssetTransfer
        {
            AssetId = asset.Id,
            FromArea = asset.Area ?? string.Empty,
            ToArea = request.ToArea.Trim(),
            FromResponsible = asset.Responsible,
            ToResponsible = request.ToResponsible,
            FromLocation = asset.Location,
            ToLocation = request.ToLocation,
            Reason = request.Reason.Trim(),
            RequestedByUserId = userId
        };
        _dbContext.AssetTransfers.Add(transfer);
        _dbContext.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Solicitar transferencia", EntityName = nameof(AssetTransfer), Description = $"Transferencia solicitada para {asset.AssetCode}." });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transfer;
    }

    public async Task<AssetTransfer?> ApproveAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var transfer = await _dbContext.AssetTransfers.Include(t => t.Asset).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (transfer is null) return null;
        if (transfer.Status != "Pendiente") throw new InvalidOperationException("La transferencia ya fue procesada.");
        transfer.Status = "Aprobada";
        transfer.ApprovedByUserId = userId;
        transfer.ApprovedAt = DateTime.UtcNow;
        transfer.Asset.Area = transfer.ToArea;
        transfer.Asset.Responsible = transfer.ToResponsible;
        transfer.Asset.Location = transfer.ToLocation;
        transfer.Asset.UpdatedAt = DateTime.UtcNow;
        _dbContext.AssetMovements.Add(new AssetMovement { AssetId = transfer.AssetId, MovementType = "Transferencia", Description = transfer.Reason, FromArea = transfer.FromArea, ToArea = transfer.ToArea, FromResponsible = transfer.FromResponsible, ToResponsible = transfer.ToResponsible, FromLocation = transfer.FromLocation, ToLocation = transfer.ToLocation, PerformedByUserId = userId });
        _dbContext.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Aprobar transferencia", EntityName = nameof(AssetTransfer), EntityId = id.ToString(), Description = $"Transferencia del bien {transfer.Asset.AssetCode} aprobada." });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transfer;
    }

    public async Task<AssetTransfer?> RejectAsync(int id, int userId, string? reason, CancellationToken cancellationToken = default)
    {
        var transfer = await _dbContext.AssetTransfers.FindAsync([id], cancellationToken);
        if (transfer is null) return null;
        if (transfer.Status != "Pendiente") throw new InvalidOperationException("La transferencia ya fue procesada.");
        transfer.Status = "Rechazada";
        _dbContext.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Rechazar transferencia", EntityName = nameof(AssetTransfer), EntityId = id.ToString(), Description = reason ?? "Transferencia rechazada." });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transfer;
    }
}

public sealed class InventoryService : IInventoryService
{
    private readonly AppDbContext _dbContext;
    public InventoryService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<InventorySession>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventorySessions.AsNoTracking().Include(i => i.CreatedByUser).OrderByDescending(i => i.StartedAt).ToListAsync(cancellationToken);
    }
}

public sealed class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    public AuditService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<AuditRecord>> ListAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditRecords.AsNoTracking().Include(a => a.Area).Include(a => a.CreatedByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        return await query.OrderByDescending(a => a.ScheduledDate).ToListAsync(cancellationToken);
    }
}

public sealed class CatalogService : ICatalogService
{
    private readonly AppDbContext _dbContext;
    public CatalogService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<InstitutionalArea>> GetAreasAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.InstitutionalAreas.AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(a => a.Active);
        return await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PhysicalLocation>> GetLocationsAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PhysicalLocations.AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(l => l.Active);
        return await query.OrderBy(l => l.Site).ThenBy(l => l.Name).ToListAsync(cancellationToken);
    }
}