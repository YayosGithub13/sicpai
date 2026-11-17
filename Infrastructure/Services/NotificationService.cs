using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Notification>> ListAsync(int userId, bool unreadOnly, CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (unreadOnly) query = query.Where(n => !n.Read);
        return await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(cancellationToken);
    }

    public async Task<Notification?> MarkReadAsync(long id, int userId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
        if (notification is null) return null;
        notification.Read = true;
        await _db.SaveChangesAsync(cancellationToken);
        return notification;
    }
}
