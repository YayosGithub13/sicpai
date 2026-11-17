namespace Domain.DTOs;

public sealed record AssetMovementListDto(
    int Id,
    DateTime OccurredAt,
    string MovementType,
    string Description,
    string? FromArea,
    string? ToArea,
    int AssetId,
    string AssetCode,
    string AssetName,
    int? UserId,
    string? UserFullName);

public sealed record AssetAttachmentDownloadDto(
    int AssetId,
    int AttachmentId,
    string StoredFileName,
    string OriginalFileName,
    string ContentType);

public sealed record AssetImportRowDto(
    int Row,
    string Code,
    string Name,
    string AssetType,
    string SerialNumber,
    string Area,
    string Responsible,
    string Location,
    string Status,
    decimal AcquisitionValue);

public sealed record AssetImportRejectedRowDto(int Row, string Code, string Reason);

public sealed record AssetAttachmentUploadDto(
    int AttachmentId,
    string RelativePath,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long Size);
