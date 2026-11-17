namespace SICPAI.ViewModels;

public sealed class AssetCreateViewModel
{
    public string AssetCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string PhysicalCondition { get; set; } = "Bueno";
    public string Status { get; set; } = "Activo";
    public string? Area { get; set; }
    public string? Responsible { get; set; }
    public string? Location { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public decimal AcquisitionValue { get; set; }
}