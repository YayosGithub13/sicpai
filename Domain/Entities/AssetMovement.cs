namespace Domain.Entities;

public class AssetMovement
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FromArea { get; set; }
    public string? ToArea { get; set; }
    public string? FromResponsible { get; set; }
    public string? ToResponsible { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public int? PerformedByUserId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Asset Asset { get; set; } = null!;
    public User? PerformedByUser { get; set; }
}
