namespace server.Models;

public class StoredS3FileResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string S3Key { get; set; } = string.Empty;

    public StoredS3FileStatus Status { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? LastRetrieved { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string PresignedUrl { get; set; } = string.Empty;
}
