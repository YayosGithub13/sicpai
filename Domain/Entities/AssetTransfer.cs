namespace Domain.Entities;

public class AssetTransfer
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string FromArea { get; set; } = string.Empty;
    public string ToArea { get; set; } = string.Empty;
    public string? FromResponsible { get; set; }
    public string? ToResponsible { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendiente";
    public int RequestedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }

    public Asset Asset { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
