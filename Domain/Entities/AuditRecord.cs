namespace Domain.Entities;

public class AuditRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Status { get; set; } = "Programada";
    public int? AreaId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public InstitutionalArea? Area { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<AuditAssetResult> AssetResults { get; set; } = new List<AuditAssetResult>();
}
