using Domain.Entities;

namespace Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> ListAsync(int userId, bool unreadOnly, CancellationToken cancellationToken = default);
    Task<Notification?> MarkReadAsync(long id, int userId, CancellationToken cancellationToken = default);
}
