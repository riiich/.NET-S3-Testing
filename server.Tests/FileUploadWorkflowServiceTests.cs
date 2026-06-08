using Microsoft.Extensions.Logging;
using server.Interfaces;
using server.Models;
using server.Services;

namespace server.Tests;

public class FileUploadWorkflowServiceTests
{
    [Fact]
    public async Task UploadAsync_RequiresOwnerId()
    {
        FileUploadWorkflowService service = new(
            new TestLogger<FileUploadWorkflowService>(),
            new FakeS3FileUploadService(),
            new FakeS3MetadataRepository());

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadAsync("", CreatePdfInput()));

        Assert.Contains("owner id", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_CreatesMetadataWithPendingStatusBeforeUploading()
    {
        FakeS3MetadataRepository repository = new();
        FakeS3FileUploadService storage = new();
        FileUploadWorkflowService service = new(new TestLogger<FileUploadWorkflowService>(), storage, repository);

        S3Metadata result = await service.UploadAsync("owner-1", CreatePdfInput());

        Assert.Equal("owner-1", repository.CreatedItem?.OwnerId);
        Assert.Equal(S3MetadataStatus.Uploaded, result.Status);
        Assert.Equal("uploads/generated.pdf", result.S3Key);
        Assert.Single(repository.UpdatedItems);
    }

    [Fact]
    public async Task UploadAsync_MarksMetadataFailedWhenS3UploadFails()
    {
        FakeS3MetadataRepository repository = new();
        FakeS3FileUploadService storage = new()
        {
            UploadException = new InvalidOperationException("upload failed")
        };
        FileUploadWorkflowService service = new(new TestLogger<FileUploadWorkflowService>(), storage, repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UploadAsync("owner-1", CreatePdfInput()));

        Assert.Equal(S3MetadataStatus.Failed, repository.CreatedItem?.Status);
        Assert.Single(repository.UpdatedItems);
    }

    private static FileUploadInput CreatePdfInput()
    {
        return new FileUploadInput
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Length = 8,
            Content = new MemoryStream("%PDF-1.7"u8.ToArray())
        };
    }

    private sealed class FakeS3FileUploadService : IS3FileUploadService
    {
        public Exception? UploadException { get; set; }

        public string CreateObjectKey(string fileName)
        {
            return "uploads/generated.pdf";
        }

        public Task UploadFileAsync(FileUploadInput file, string s3Key)
        {
            if (UploadException is not null)
            {
                throw UploadException;
            }

            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(string s3Key)
        {
            return Task.CompletedTask;
        }

        public string GetPresignedUrl(string s3Key)
        {
            return $"https://example.test/{s3Key}";
        }
    }

    private sealed class FakeS3MetadataRepository : IS3MetadataRepository
    {
        public S3Metadata? CreatedItem { get; private set; }

        public List<S3Metadata> UpdatedItems { get; } = [];

        public Task<S3Metadata> CreateAsync(S3Metadata item)
        {
            item.Id = 123;
            CreatedItem = item;

            return Task.FromResult(item);
        }

        public Task<S3Metadata> UpdateAsync(S3Metadata item)
        {
            UpdatedItems.Add(item);

            return Task.FromResult(item);
        }

        public Task<S3Metadata?> GetByIdAsync(int id)
        {
            return Task.FromResult<S3Metadata?>(null);
        }

        public Task<S3Metadata?> GetByOwnerIdAndIdAsync(string ownerId, int s3MetadataId)
        {
            return Task.FromResult<S3Metadata?>(null);
        }

        public Task<IReadOnlyList<S3Metadata>> GetByOwnerIdAsync(string ownerId)
        {
            return Task.FromResult<IReadOnlyList<S3Metadata>>([]);
        }

        public Task<S3Metadata?> UpdateLastRetrievedAsync(int id)
        {
            return Task.FromResult<S3Metadata?>(null);
        }

        public Task DeleteAsync(S3Metadata item)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}