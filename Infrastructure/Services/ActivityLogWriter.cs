using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services;

public sealed class ActivityLogWriter : IActivityLogWriter
{
    private readonly AppDbContext _db;

    public ActivityLogWriter(AppDbContext db) => _db = db;

    public async Task WriteAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
