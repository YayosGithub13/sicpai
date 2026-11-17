using Domain.DTOs;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int? userId, CancellationToken cancellationToken = default);
}

public sealed record DashboardSummaryDto(
    int Total,
    int Active,
    int Repair,
    int Missing,
    IReadOnlyList<DashboardGroupDto> ByStatus,
    IReadOnlyList<DashboardGroupDto> ByArea,
    int PendingTransfers,
    int UnreadNotifications);

public sealed record DashboardGroupDto(string Name, int Total);
