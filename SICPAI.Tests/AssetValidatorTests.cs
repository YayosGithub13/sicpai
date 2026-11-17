using Application.Validators;
using Domain.DTOs;

namespace SICPAI.Tests;

public class AssetValidatorTests
{
    [Fact]
    public void Validate_WhenRequiredFieldsAreMissing_ReturnsExpectedErrors()
    {
        var dto = new CreateAssetDto(
            AssetCode: "",
            Barcode: "",
            Name: "",
            AssetType: "",
            SerialNumber: null,
            Brand: null,
            Model: null,
            PhysicalCondition: "Bueno",
            Status: "Activo",
            Area: null,
            Responsible: null,
            Location: null,
            AcquisitionDate: null,
            AcquisitionValue: -10);

        var result = AssetValidator.Validate(dto);

        Assert.Contains(nameof(dto.AssetCode), result.Keys);
        Assert.Contains(nameof(dto.Name), result.Keys);
        Assert.Contains(nameof(dto.AssetType), result.Keys);
        Assert.Contains(nameof(dto.AcquisitionValue), result.Keys);
    }

    [Fact]
    public void Validate_WhenDataIsValid_ReturnsNoErrors()
    {
        var dto = new CreateAssetDto(
            AssetCode: "EQ-001",
            Barcode: "BC-001",
            Name: "Laptop",
            AssetType: "Cómputo",
            SerialNumber: "SN-001",
            Brand: "Dell",
            Model: "Latitude",
            PhysicalCondition: "Bueno",
            Status: "Activo",
            Area: "TI",
            Responsible: "Usuario",
            Location: "Oficina",
            AcquisitionDate: DateTime.UtcNow.Date,
            AcquisitionValue: 1000);

        var result = AssetValidator.Validate(dto);

        Assert.Empty(result);
    }
}
