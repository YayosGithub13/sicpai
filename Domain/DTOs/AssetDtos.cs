namespace Domain.DTOs;

public sealed record AssetListDto(
    int Id,
    string AssetCode,
    string Name,
    string AssetType,
    string? SerialNumber,
    string? Area,
    string? Responsible,
    string? Location,
    string Status,
    decimal AcquisitionValue);

public sealed record AssetMovementDto(
    int Id,
    string MovementType,
    string Description,
    DateTime OccurredAt,
    string? FromArea,
    string? ToArea,
    string? FromResponsible,
    string? ToResponsible,
    string? FromLocation,
    string? ToLocation,
    string? PerformedBy);

public sealed record AssetDetailDto(
    int Id,
    string AssetCode,
    string Barcode,
    string Name,
    string AssetType,
    string? SerialNumber,
    string? Brand,
    string? Model,
    string PhysicalCondition,
    string Status,
    string? Area,
    string? Responsible,
    string? Location,
    DateTime? AcquisitionDate,
    decimal AcquisitionValue,
    IReadOnlyList<AssetMovementDto> Movements);

public sealed record CreateAssetDto(
    string AssetCode,
    string Barcode,
    string Name,
    string AssetType,
    string? SerialNumber,
    string? Brand,
    string? Model,
    string PhysicalCondition,
    string Status,
    string? Area,
    string? Responsible,
    string? Location,
    DateTime? AcquisitionDate,
    decimal AcquisitionValue);