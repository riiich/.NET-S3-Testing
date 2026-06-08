using Microsoft.Extensions.Options;
using server.Models;
using server.Services;

namespace server.Tests;

public class S3FileUploadServiceTests
{
    [Fact]
    public async Task UploadFileAsync_RejectsNonPdfContentType()
    {
        S3FileUploadService service = CreateService();
        FileUploadInput input = CreateUploadInput("report.pdf", "text/plain", "%PDF-1.7");

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains("Only PDF and CSV files", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_RejectsNonPdfExtension()
    {
        S3FileUploadService service = CreateService();
        FileUploadInput input = CreateUploadInput("report.txt", "application/pdf", "%PDF-1.7");

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains(".pdf extension", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_RejectsSpoofedPdfContent()
    {
        S3FileUploadService service = CreateService();
        FileUploadInput input = CreateUploadInput("report.pdf", "application/pdf", "not a real pdf");

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains("not a valid PDF", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_RejectsOversizedFiles()
    {
        S3FileUploadService service = CreateService(new UploadSettings
        {
            MaxFileSizeBytes = 4
        });
        FileUploadInput input = CreateUploadInput("report.pdf", "application/pdf", "%PDF-1.7");

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains("maximum allowed size", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_RejectsNonSeekableStreams()
    {
        S3FileUploadService service = CreateService();
        FileUploadInput input = new()
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Length = 8,
            Content = new NonSeekableStream("%PDF-1.7"u8.ToArray())
        };

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains("seekable", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_AcceptsValidPdfBeforeCheckingS3Configuration()
    {
        S3FileUploadService service = CreateService();
        FileUploadInput input = CreateUploadInput("report.pdf", "application/pdf", "%PDF-1.7");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UploadFileAsync(input, "uploads/test.pdf"));

        Assert.Contains("bucket name", exception.Message);
    }

    [Fact]
    public void CreateObjectKey_GeneratesServerOwnedKeyWithoutOriginalFileName()
    {
        S3FileUploadService service = CreateService();

        string key = service.CreateObjectKey(@"..\sensitive\report.pdf");

        Assert.StartsWith("uploads/", key);
        Assert.EndsWith(".pdf", key);
        Assert.DoesNotContain("report", key, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..", key);
    }

    private static S3FileUploadService CreateService(UploadSettings? uploadSettings = null)
    {
        return new S3FileUploadService(
            s3Client: null!,
            s3SettingsOptions: Options.Create(new S3Settings()),
            uploadSettingsOptions: Options.Create(uploadSettings ?? new UploadSettings()));
    }

    private static FileUploadInput CreateUploadInput(string fileName, string contentType, string content)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);

        return new FileUploadInput
        {
            FileName = fileName,
            ContentType = contentType,
            Length = bytes.Length,
            Content = new MemoryStream(bytes)
        };
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] buffer)
            : base(buffer)
        {
        }

        public override bool CanSeek => false;
    }
}