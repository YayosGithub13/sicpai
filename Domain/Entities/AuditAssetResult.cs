namespace Domain.Entities;

public class AuditAssetResult
{
    public int Id { get; set; }
    public int AuditRecordId { get; set; }
    public int AssetId { get; set; }
    public string Result { get; set; } = "Pendiente";
    public string? Notes { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByUserId { get; set; }

    public AuditRecord AuditRecord { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
