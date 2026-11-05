namespace Domain.Entities;

public class AssetAttachment
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int? UploadedByUserId { get; set; }

    public Asset Asset { get; set; } = null!;
}
