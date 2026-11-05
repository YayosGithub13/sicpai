namespace Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string PhysicalCondition { get; set; } = "Bueno";
    public string Status { get; set; } = "Activo";
    public string? Area { get; set; }
    public string? Responsible { get; set; }
    public string? Location { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public decimal AcquisitionValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetMovement> Movements { get; set; } = new List<AssetMovement>();
    public ICollection<AssetAttachment> Attachments { get; set; } = new List<AssetAttachment>();
}
