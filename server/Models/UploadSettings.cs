namespace server.Models
{
    public class UploadSettings
    {
        public string[] AcceptedTypes { get; set; } = ["application/pdf", "text/csv"];

        public string[] AcceptedExtensions { get; set; } = [".pdf", ".csv"];

        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    }
}
