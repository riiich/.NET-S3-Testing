using server.Models;

namespace server.DTOs;

public class S3MetadataResponseDto
{
    public int Id { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public S3MetadataStatus Status { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? LastRetrieved { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string FileUrl { get; set; } = string.Empty;
}