namespace server.Models;

public class S3Item
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string S3Key { get; set; } = string.Empty;

    public S3ItemStatus Status { get; set; } = S3ItemStatus.Pending;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastRetrieved { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public User? User { get; set; }
}
