using Microsoft.AspNetCore.Mvc;
using server.Controllers;
using server.Interfaces;
using server.Models;

namespace server.Tests;

public class S3MetadataControllerTests
{
    [Fact]
    public async Task GetByOwnerId_ReturnsNotFoundWhenOwnerHasNoMetadata()
    {
        S3MetadataController controller = new(new FakeS3MetadataService(), new FakeS3FileUploadService());

        IActionResult result = await controller.GetByOwnerId("missing-owner");

        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No S3 metadata exists for this owner.", notFound.Value);
    }

    [Fact]
    public async Task Delete_DoesNotDeleteFromS3WhenOwnerMetadataDoesNotExist()
    {
        FakeS3FileUploadService storage = new();
        S3MetadataController controller = new(new FakeS3MetadataService(), storage);

        IActionResult result = await controller.Delete("owner-1", 404);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(storage.DeleteWasCalled);
    }

    [Fact]
    public async Task Delete_DeletesFromS3BeforeDeletingMetadata()
    {
        FakeS3MetadataService metadataService = new()
        {
            OwnerMetadata = new S3Metadata
            {
                Id = 7,
                OwnerId = "owner-1",
                S3Key = "uploads/file.pdf"
            }
        };
        FakeS3FileUploadService storage = new();
        S3MetadataController controller = new(metadataService, storage);

        IActionResult result = await controller.Delete("owner-1", 7);

        Assert.IsType<NoContentResult>(result);
        Assert.True(storage.DeleteWasCalled);
        Assert.True(metadataService.DeleteWasCalled);
    }

    [Fact]
    public async Task RetrieveFile_MarksMetadataRetrievedBeforeRedirecting()
    {
        FakeS3MetadataService metadataService = new()
        {
            OwnerMetadata = new S3Metadata
            {
                Id = 7,
                OwnerId = "owner-1",
                S3Key = "uploads/file.pdf"
            }
        };
        S3MetadataController controller = new(metadataService, new FakeS3FileUploadService());

        IActionResult result = await controller.RetrieveFile("owner-1", 7);

        RedirectResult redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://example.test/uploads/file.pdf", redirect.Url);
        Assert.True(metadataService.MarkRetrievedWasCalled);
    }

    private sealed class FakeS3MetadataService : IS3MetadataService
    {
        public S3Metadata? OwnerMetadata { get; set; }

        public bool DeleteWasCalled { get; private set; }

        public bool MarkRetrievedWasCalled { get; private set; }

        public Task<S3Metadata?> GetByIdAsync(int id)
        {
            return Task.FromResult<S3Metadata?>(null);
        }

        public Task<S3Metadata?> GetByOwnerIdAndIdAsync(string ownerId, int s3MetadataId)
        {
            if (OwnerMetadata?.OwnerId == ownerId && OwnerMetadata.Id == s3MetadataId)
            {
                return Task.FromResult<S3Metadata?>(OwnerMetadata);
            }

            return Task.FromResult<S3Metadata?>(null);
        }

        public Task<IReadOnlyList<S3Metadata>> GetByOwnerIdAsync(string ownerId)
        {
            return Task.FromResult<IReadOnlyList<S3Metadata>>([]);
        }

        public Task DeleteAsync(S3Metadata item)
        {
            DeleteWasCalled = true;

            return Task.CompletedTask;
        }

        public Task<S3Metadata?> MarkRetrievedAsync(int id)
        {
            MarkRetrievedWasCalled = true;

            if (OwnerMetadata?.Id == id)
            {
                OwnerMetadata.LastRetrieved = DateTime.UtcNow;
                return Task.FromResult<S3Metadata?>(OwnerMetadata);
            }

            return Task.FromResult<S3Metadata?>(null);
        }
    }

    private sealed class FakeS3FileUploadService : IS3FileUploadService
    {
        public bool DeleteWasCalled { get; private set; }

        public string CreateObjectKey(string fileName)
        {
            return "uploads/file.pdf";
        }

        public Task ValidateFileAsync(FileUploadInput file)
        {
            return Task.CompletedTask;
        }

        public Task UploadFileAsync(FileUploadInput file, string s3Key)
        {
            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(string s3Key)
        {
            DeleteWasCalled = true;

            return Task.CompletedTask;
        }

        public string GetPresignedUrl(string s3Key)
        {
            return $"https://example.test/{s3Key}";
        }
    }
}