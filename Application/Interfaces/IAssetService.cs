using Domain.DTOs;

namespace Application.Interfaces;

public interface IAssetService
{
    Task<IReadOnlyList<AssetListDto>> ListAsync(string? search, string? status, string? area, CancellationToken cancellationToken = default);
    Task<AssetDetailDto?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<AssetDetailDto?> ScanAsync(string code, CancellationToken cancellationToken = default);
    Task<AssetDetailDto> CreateAsync(CreateAssetDto request, int? userId, CancellationToken cancellationToken = default);
    Task<AssetDetailDto?> UpdateAsync(int id, CreateAssetDto request, int? userId, CancellationToken cancellationToken = default);
    Task<AssetMovementDto?> AddMovementAsync(int assetId, string movementType, string description, string? toArea, string? toResponsible, string? toLocation, int? userId, CancellationToken cancellationToken = default);
    Task<AssetDetailDto?> SetMaintenanceAsync(int assetId, string description, int? userId, CancellationToken cancellationToken = default);
    Task<bool?> RetireAsync(int assetId, string reason, int? userId, CancellationToken cancellationToken = default);
    Task<(int Total, IReadOnlyList<AssetMovementListDto> Items)> ListMovementsAsync(string? search, string? type, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AssetAttachmentDownloadDto?> GetAttachmentDownloadAsync(int assetId, int attachmentId, CancellationToken cancellationToken = default);
    Task<(int Imported, IReadOnlyList<AssetImportRejectedRowDto> Rejected)> ImportAsync(IReadOnlyList<AssetImportRowDto> rows, CancellationToken cancellationToken = default);
    Task<AssetAttachmentUploadDto?> AttachDocumentAsync(int assetId, string originalFileName, string storedFileName, string relativePath, string contentType, long size, int? userId, CancellationToken cancellationToken = default);
}