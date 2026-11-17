using Domain.DTOs;

namespace Application.Validators;

public static class AssetValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(CreateAssetDto request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.AssetCode)) errors[nameof(request.AssetCode)] = ["El código patrimonial es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.Name)) errors[nameof(request.Name)] = ["El nombre del bien es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.AssetType)) errors[nameof(request.AssetType)] = ["El tipo de activo es obligatorio."];
        if (request.AcquisitionValue < 0) errors[nameof(request.AcquisitionValue)] = ["El valor no puede ser negativo."];
        return errors;
    }
}