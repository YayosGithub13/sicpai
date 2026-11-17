using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _dbContext;
    public AuditLogService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<(int Total, IReadOnlyList<ActivityLog> Items)> ListAsync(string? action, string? entity, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _dbContext.ActivityLogs.AsNoTracking().Include(l => l.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(l => l.Action == action);
        if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(l => l.EntityName == entity);
        if (from.HasValue) query = query.Where(l => l.OccurredAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(l => l.OccurredAt < to.Value.ToUniversalTime().AddDays(1));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(l => l.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (total, items);
    }
}

public sealed class ReportAssetService : IReportAssetService
{
    private readonly AppDbContext _dbContext;
    public ReportAssetService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<Asset>> ListAsync(string? status, string? area, string? responsible, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(area)) query = query.Where(a => a.Area == area);
        if (!string.IsNullOrWhiteSpace(responsible)) query = query.Where(a => a.Responsible == responsible);
        return await query.OrderBy(a => a.AssetCode).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetMovement>> ListMovementsAsync(string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AssetMovements
            .AsNoTracking()
            .Include(m => m.Asset)
            .Include(m => m.PerformedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(m => m.MovementType == type);
        if (from.HasValue) query = query.Where(m => m.OccurredAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(m => m.OccurredAt < to.Value.ToUniversalTime().AddDays(1));

        return await query.OrderByDescending(m => m.OccurredAt).ToListAsync(cancellationToken);
    }
}