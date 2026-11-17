using Domain.Entities;

namespace Application.Interfaces;

public interface IAuditLogService
{
    Task<(int Total, IReadOnlyList<ActivityLog> Items)> ListAsync(string? action, string? entity, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IActivityLogWriter
{
    Task WriteAsync(ActivityLog log, CancellationToken cancellationToken = default);
}

public interface IReportAssetService
{
    Task<IReadOnlyList<Asset>> ListAsync(string? status, string? area, string? responsible, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssetMovement>> ListMovementsAsync(string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}