using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class InventoryOperations : IInventoryOperations
{
    private readonly AppDbContext _db;
    public InventoryOperations(AppDbContext db) => _db = db;

    public Task<InventorySession?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _db.InventorySessions.AsNoTracking().Include(i => i.Counts).ThenInclude(c => c.Asset).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<InventorySession?> StartAsync(string name, string? area, int userId, CancellationToken cancellationToken = default)
    {
        var assets = await _db.Assets.Where(a => string.IsNullOrWhiteSpace(area) || a.Area == area).ToListAsync(cancellationToken);
        var session = new InventorySession { Name = name.Trim(), CreatedByUserId = userId, Counts = assets.Select(a => new InventoryCount { AssetId = a.Id }).ToList() };
        _db.InventorySessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<InventoryCount?> CountAsync(int sessionId, int assetId, string result, string? notes, int? userId, CancellationToken cancellationToken = default)
    {
        var count = await _db.InventoryCounts.FirstOrDefaultAsync(c => c.InventorySessionId == sessionId && c.AssetId == assetId, cancellationToken);
        if (count is null) return null;
        count.Result = result; count.Notes = notes; count.CountedAt = DateTime.UtcNow; count.CountedByUserId = userId;
        await _db.SaveChangesAsync(cancellationToken);
        return count;
    }

    public async Task<InventorySession?> CloseAsync(int id, int? userId, CancellationToken cancellationToken = default)
    {
        var session = await _db.InventorySessions.FindAsync([id], cancellationToken);
        if (session is null) return null;
        session.Status = "Cerrado"; session.ClosedAt = DateTime.UtcNow;
        var differences = await _db.InventoryCounts.Where(c => c.InventorySessionId == id && c.Result != "Encontrado").Include(c => c.Asset).ToListAsync(cancellationToken);
        foreach (var difference in differences)
        {
            if (difference.Result == "Dañado") difference.Asset.Status = "Dañado";
            if (difference.Result == "Faltante") difference.Asset.Status = "Extraviado";
            _db.Notifications.Add(new Notification { UserId = session.CreatedByUserId, Type = "Inventario", Title = $"Bien {difference.Result.ToLowerInvariant()}", Message = $"{difference.Asset.AssetCode}: {difference.Asset.Name}", Link = "/Operaciones/Auditorias" });
            _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Conciliación", EntityName = nameof(InventoryCount), EntityId = difference.Id.ToString(), Description = $"Bien marcado como {difference.Result}." });
        }
        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }
}

public sealed class AuditOperations : IAuditOperations
{
    private readonly AppDbContext _db;
    public AuditOperations(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AuditAssetResult>?> GetAssetsAsync(int id, CancellationToken cancellationToken = default)
    {
        var audit = await _db.AuditRecords.AsNoTracking().Include(a => a.AssetResults).ThenInclude(r => r.Asset).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        return audit?.AssetResults.ToList();
    }

    public async Task<AuditAssetResult?> AddAssetAsync(int id, int assetId, CancellationToken cancellationToken = default)
    {
        if (!await _db.AuditRecords.AnyAsync(a => a.Id == id, cancellationToken) || !await _db.Assets.AnyAsync(a => a.Id == assetId, cancellationToken)) return null;
        if (await _db.AuditAssetResults.AnyAsync(r => r.AuditRecordId == id && r.AssetId == assetId, cancellationToken)) throw new InvalidOperationException("El bien ya está incluido en la auditoría.");
        var result = new AuditAssetResult { AuditRecordId = id, AssetId = assetId };
        _db.AuditAssetResults.Add(result); await _db.SaveChangesAsync(cancellationToken); return result;
    }

    public async Task<AuditAssetResult?> VerifyAssetAsync(int id, int assetId, string result, string? notes, int? userId, CancellationToken cancellationToken = default)
    {
        var item = await _db.AuditAssetResults.FirstOrDefaultAsync(r => r.AuditRecordId == id && r.AssetId == assetId, cancellationToken);
        if (item is null) return null;
        item.Result = result; item.Notes = notes; item.VerifiedAt = DateTime.UtcNow; item.VerifiedByUserId = userId;
        await _db.SaveChangesAsync(cancellationToken); return item;
    }

    public async Task<AuditRecord?> CreateAsync(string name, string? description, DateTime scheduledDate, int? areaId, int userId, CancellationToken cancellationToken = default)
    {
        if (areaId.HasValue && !await _db.InstitutionalAreas.AnyAsync(a => a.Id == areaId, cancellationToken)) return null;
        var audit = new AuditRecord { Name = name.Trim(), Description = description?.Trim(), ScheduledDate = scheduledDate, AreaId = areaId, CreatedByUserId = userId, Status = "Programada" };
        _db.AuditRecords.Add(audit); _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Crear", EntityName = nameof(AuditRecord), Description = $"Auditoría {audit.Name} programada." });
        await _db.SaveChangesAsync(cancellationToken); return audit;
    }

    public async Task<AuditRecord?> UpdateStatusAsync(int id, string status, int? userId, CancellationToken cancellationToken = default)
    {
        var audit = await _db.AuditRecords.FindAsync([id], cancellationToken); if (audit is null) return null;
        audit.Status = status; audit.ClosedDate = status is "Completada" or "Cancelada" ? DateTime.UtcNow : null;
        _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Actualizar estado", EntityName = nameof(AuditRecord), EntityId = id.ToString(), Description = $"Estado cambiado a {status}." });
        await _db.SaveChangesAsync(cancellationToken); return audit;
    }
}
