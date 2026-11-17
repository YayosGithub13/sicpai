using Application.Interfaces;
using Application.Validators;
using Domain.DTOs;
using Domain.Entities;

namespace Application.Services;

public sealed class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IActivityLogWriter _activityLogWriter;

    public AssetService(IAssetRepository assetRepository, IActivityLogWriter activityLogWriter)
    {
        _assetRepository = assetRepository;
        _activityLogWriter = activityLogWriter;
    }

    public async Task<IReadOnlyList<AssetListDto>> ListAsync(string? search, string? status, string? area, CancellationToken cancellationToken = default)
    {
        var assets = await _assetRepository.ListAsync(search, status, area, cancellationToken);
        return assets.Select(MapList).ToList();
    }

    public async Task<AssetDetailDto?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(id, includeHistory: true, cancellationToken);
        return asset is null ? null : MapDetail(asset);
    }

    public async Task<AssetDetailDto?> ScanAsync(string code, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.FindByCodeAsync(code, cancellationToken);
        if (asset is null) return null;
        var completeAsset = await _assetRepository.GetAsync(asset.Id, includeHistory: true, cancellationToken);
        return completeAsset is null ? null : MapDetail(completeAsset);
    }

    public async Task<AssetDetailDto> CreateAsync(CreateAssetDto request, int? userId, CancellationToken cancellationToken = default)
    {
        var errors = AssetValidator.Validate(request);
        if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors.SelectMany(e => e.Value)), nameof(request));
        if (await _assetRepository.ExistsCodeAsync(request.AssetCode, cancellationToken: cancellationToken))
            throw new InvalidOperationException("El código patrimonial ya existe.");
        if (!string.IsNullOrWhiteSpace(request.Barcode) && await _assetRepository.ExistsCodeAsync(request.Barcode, cancellationToken: cancellationToken))
            throw new InvalidOperationException("El código de barras ya existe.");

        var now = DateTime.UtcNow;
        var asset = new Asset
        {
            AssetCode = request.AssetCode.Trim(),
            Barcode = request.Barcode.Trim(),
            Name = request.Name.Trim(),
            AssetType = request.AssetType.Trim(),
            SerialNumber = request.SerialNumber,
            Brand = request.Brand,
            Model = request.Model,
            PhysicalCondition = request.PhysicalCondition,
            Status = request.Status,
            Area = request.Area,
            Responsible = request.Responsible,
            Location = request.Location,
            AcquisitionDate = request.AcquisitionDate,
            AcquisitionValue = request.AcquisitionValue,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _assetRepository.AddAsync(asset, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);
        return MapDetail(asset);
    }

    public async Task<AssetDetailDto?> UpdateAsync(int id, CreateAssetDto request, int? userId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(id, includeHistory: true, cancellationToken);
        if (asset is null) return null;
        if (await _assetRepository.ExistsCodeAsync(request.AssetCode, id, cancellationToken))
            throw new InvalidOperationException("El código patrimonial ya existe.");

        var changes = new List<string>();
        if (asset.Area != request.Area || asset.Responsible != request.Responsible || asset.Location != request.Location)
            changes.Add("Se actualizó la asignación del bien.");
        if (asset.Status != request.Status) changes.Add($"Cambio de estado: {asset.Status} → {request.Status}.");

        asset.AssetCode = request.AssetCode.Trim();
        asset.Barcode = request.Barcode.Trim();
        asset.SerialNumber = request.SerialNumber?.Trim();
        asset.Name = request.Name.Trim();
        asset.AssetType = request.AssetType.Trim();
        asset.Brand = request.Brand?.Trim();
        asset.Model = request.Model?.Trim();
        asset.PhysicalCondition = request.PhysicalCondition;
        asset.Status = request.Status;
        asset.Area = request.Area;
        asset.Responsible = request.Responsible;
        asset.Location = request.Location;
        asset.AcquisitionDate = request.AcquisitionDate;
        asset.AcquisitionValue = request.AcquisitionValue;
        asset.UpdatedAt = DateTime.UtcNow;

        foreach (var change in changes)
        {
            asset.Movements.Add(new AssetMovement { MovementType = "Actualización", Description = change, PerformedByUserId = userId });
        }

        await _activityLogWriter.WriteAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Actualizar",
            EntityName = nameof(Asset),
            EntityId = id.ToString(),
            Description = $"Bien {asset.AssetCode} actualizado."
        }, cancellationToken);

        await _assetRepository.SaveChangesAsync(cancellationToken);
        return MapDetail(asset);
    }

    public async Task<AssetMovementDto?> AddMovementAsync(int assetId, string movementType, string description, string? toArea, string? toResponsible, string? toLocation, int? userId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(assetId, cancellationToken: cancellationToken);
        if (asset is null) return null;

        var movement = new AssetMovement
        {
            AssetId = assetId,
            MovementType = movementType,
            Description = description,
            FromArea = asset.Area,
            ToArea = toArea,
            FromResponsible = asset.Responsible,
            ToResponsible = toResponsible,
            FromLocation = asset.Location,
            ToLocation = toLocation,
            PerformedByUserId = userId,
            OccurredAt = DateTime.UtcNow
        };

        asset.Area = toArea ?? asset.Area;
        asset.Responsible = toResponsible ?? asset.Responsible;
        asset.Location = toLocation ?? asset.Location;
        asset.UpdatedAt = DateTime.UtcNow;
        asset.Movements.Add(movement);

        await _activityLogWriter.WriteAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Movimiento",
            EntityName = nameof(Asset),
            EntityId = assetId.ToString(),
            Description = movement.Description
        }, cancellationToken);

        await _assetRepository.SaveChangesAsync(cancellationToken);
        return new AssetMovementDto(movement.Id, movement.MovementType, movement.Description, movement.OccurredAt, movement.FromArea, movement.ToArea, movement.FromResponsible, movement.ToResponsible, movement.FromLocation, movement.ToLocation, null);
    }

    public async Task<AssetDetailDto?> SetMaintenanceAsync(int assetId, string description, int? userId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(assetId, includeHistory: true, cancellationToken);
        if (asset is null) return null;
        asset.Status = "En reparación";
        asset.UpdatedAt = DateTime.UtcNow;
        asset.Movements.Add(new AssetMovement { AssetId = assetId, MovementType = "Mantenimiento", Description = description, PerformedByUserId = userId });

        await _activityLogWriter.WriteAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Mantenimiento",
            EntityName = nameof(Asset),
            EntityId = assetId.ToString(),
            Description = description
        }, cancellationToken);

        await _assetRepository.SaveChangesAsync(cancellationToken);
        return MapDetail(asset);
    }

    public async Task<bool?> RetireAsync(int assetId, string reason, int? userId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(assetId, includeHistory: true, cancellationToken);
        if (asset is null) return null;
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("El motivo de baja es obligatorio.", nameof(reason));

        asset.Status = "Baja definitiva";
        asset.UpdatedAt = DateTime.UtcNow;
        asset.Movements.Add(new AssetMovement { AssetId = assetId, MovementType = "Baja", Description = reason.Trim(), PerformedByUserId = userId });

        await _activityLogWriter.WriteAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Baja definitiva",
            EntityName = nameof(Asset),
            EntityId = assetId.ToString(),
            Description = reason.Trim()
        }, cancellationToken);

        await _assetRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(int Total, IReadOnlyList<AssetMovementListDto> Items)> ListMovementsAsync(string? search, string? type, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await _assetRepository.ListMovementsAsync(search, type, from, to, cancellationToken);
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new AssetMovementListDto(m.Id, m.OccurredAt, m.MovementType, m.Description, m.FromArea, m.ToArea, m.AssetId, m.Asset.AssetCode, m.Asset.Name, m.PerformedByUserId, m.PerformedByUser?.FullName))
            .ToList();
        return (total, items);
    }

    public async Task<AssetAttachmentDownloadDto?> GetAttachmentDownloadAsync(int assetId, int attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _assetRepository.GetAttachmentAsync(assetId, attachmentId, cancellationToken);
        return attachment is null
            ? null
            : new AssetAttachmentDownloadDto(assetId, attachment.Id, attachment.StoredFileName, attachment.OriginalFileName, attachment.ContentType);
    }

    public async Task<(int Imported, IReadOnlyList<AssetImportRejectedRowDto> Rejected)> ImportAsync(IReadOnlyList<AssetImportRowDto> rows, CancellationToken cancellationToken = default)
    {
        var imported = 0;
        var rejected = new List<AssetImportRejectedRowDto>();
        var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pendingAssets = new List<Asset>();

        static string Normalize(string? value) => value?.Trim() ?? string.Empty;

        foreach (var row in rows)
        {
            var code = Normalize(row.Code);
            var name = Normalize(row.Name);
            var assetType = Normalize(row.AssetType);
            var serialNumber = Normalize(row.SerialNumber);
            var area = Normalize(row.Area);
            var responsible = Normalize(row.Responsible);
            var location = Normalize(row.Location);
            var status = Normalize(row.Status);

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                rejected.Add(new AssetImportRejectedRowDto(row.Row, code, "Código y nombre son obligatorios."));
                continue;
            }

            if (!codesInFile.Add(code))
            {
                rejected.Add(new AssetImportRejectedRowDto(row.Row, code, "Código duplicado dentro del archivo de importación."));
                continue;
            }

            if (await _assetRepository.ExistsCodeAsync(code, cancellationToken: cancellationToken))
            {
                rejected.Add(new AssetImportRejectedRowDto(row.Row, code, "El código ya existe en el sistema."));
                continue;
            }

            if (row.AcquisitionValue < 0)
            {
                rejected.Add(new AssetImportRejectedRowDto(row.Row, code, "El valor de adquisición no puede ser negativo."));
                continue;
            }

            pendingAssets.Add(new Asset
            {
                AssetCode = code,
                Barcode = code,
                Name = name,
                AssetType = string.IsNullOrWhiteSpace(assetType) ? "General" : assetType,
                SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber,
                Area = string.IsNullOrWhiteSpace(area) ? null : area,
                Responsible = string.IsNullOrWhiteSpace(responsible) ? null : responsible,
                Location = string.IsNullOrWhiteSpace(location) ? null : location,
                Status = string.IsNullOrWhiteSpace(status) ? "Activo" : status,
                AcquisitionValue = row.AcquisitionValue,
                UpdatedAt = DateTime.UtcNow
            });
        }

        foreach (var asset in pendingAssets)
        {
            await _assetRepository.AddAsync(asset, cancellationToken);
            imported++;
        }

        if (imported > 0)
        {
            await _assetRepository.SaveChangesAsync(cancellationToken);
        }

        return (imported, rejected);
    }

    public async Task<AssetAttachmentUploadDto?> AttachDocumentAsync(int assetId, string originalFileName, string storedFileName, string relativePath, string contentType, long size, int? userId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetAsync(assetId, cancellationToken: cancellationToken);
        if (asset is null) return null;
        var attachment = new AssetAttachment
        {
            AssetId = assetId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            ContentType = contentType,
            Size = size,
            UploadedByUserId = userId
        };
        await _assetRepository.AddAttachmentAsync(attachment, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);
        attachment.RelativePath = $"/api/assets/{assetId}/attachments/{attachment.Id}/download";
        await _assetRepository.SaveChangesAsync(cancellationToken);
        return new AssetAttachmentUploadDto(attachment.Id, attachment.RelativePath, originalFileName, storedFileName, contentType, size);
    }

    private static AssetListDto MapList(Asset asset) => new(
        asset.Id, asset.AssetCode, asset.Name, asset.AssetType, asset.SerialNumber,
        asset.Area, asset.Responsible, asset.Location, asset.Status, asset.AcquisitionValue);

    private static AssetDetailDto MapDetail(Asset asset) => new(
        asset.Id, asset.AssetCode, asset.Barcode, asset.Name, asset.AssetType,
        asset.SerialNumber, asset.Brand, asset.Model, asset.PhysicalCondition, asset.Status,
        asset.Area, asset.Responsible, asset.Location, asset.AcquisitionDate, asset.AcquisitionValue,
        asset.Movements.OrderByDescending(m => m.OccurredAt).Select(m => new AssetMovementDto(
            m.Id, m.MovementType, m.Description, m.OccurredAt, m.FromArea, m.ToArea,
            m.FromResponsible, m.ToResponsible, m.FromLocation, m.ToLocation,
            m.PerformedByUser?.FullName)).ToList());
}