using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetSummaryAsync(int? userId, CancellationToken cancellationToken = default)
    {
        var byStatus = await _db.Assets.AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new DashboardGroupDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
        var byArea = await _db.Assets.AsNoTracking()
            .GroupBy(a => a.Area ?? "Sin área")
            .Select(g => new DashboardGroupDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Total)
            .Take(8)
            .ToListAsync(cancellationToken);
        var unread = userId.HasValue
            ? await _db.Notifications.CountAsync(n => n.UserId == userId.Value && !n.Read, cancellationToken)
            : 0;

        return new DashboardSummaryDto(
            await _db.Assets.CountAsync(cancellationToken),
            await _db.Assets.CountAsync(a => a.Status == "Activo", cancellationToken),
            await _db.Assets.CountAsync(a => a.Status == "En reparación", cancellationToken),
            await _db.Assets.CountAsync(a => a.Status == "Extraviado", cancellationToken),
            byStatus,
            byArea,
            await _db.AssetTransfers.CountAsync(t => t.Status == "Pendiente", cancellationToken),
            unread);
    }
}
