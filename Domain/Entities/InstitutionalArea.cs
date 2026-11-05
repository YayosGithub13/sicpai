namespace Domain.Entities;

public class InstitutionalArea
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentAreaId { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public InstitutionalArea? ParentArea { get; set; }
    public ICollection<InstitutionalArea> Children { get; set; } = new List<InstitutionalArea>();
}
