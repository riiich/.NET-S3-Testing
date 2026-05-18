namespace server.Models;

public class CreateS3ItemRequest
{
    public int UserId { get; set; }

    public string S3Key { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }
}
