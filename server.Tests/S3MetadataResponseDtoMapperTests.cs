using server.DTOs;
using server.Mappers;
using server.Models;

namespace server.Tests;

public class S3MetadataResponseDtoMapperTests
{
    [Fact]
    public void MapS3MetadataToS3MetadataResponseDto_DoesNotExposeS3Key()
    {
        IEnumerable<string> propertyNames = typeof(S3MetadataResponseDto)
            .GetProperties()
            .Select(property => property.Name);

        Assert.DoesNotContain("S3Key", propertyNames);
        Assert.DoesNotContain("PresignedUrl", propertyNames);
    }

    [Fact]
    public void MapS3MetadataToS3MetadataResponseDto_MapsPublicMetadataFields()
    {
        S3Metadata item = new()
        {
            Id = 9,
            OwnerId = "owner-1",
            S3Key = "uploads/private-key.pdf",
            Status = S3MetadataStatus.Uploaded,
            UploadedAt = new DateTime(2026, 6, 8, 1, 2, 3, DateTimeKind.Utc),
            LastRetrieved = new DateTime(2026, 6, 8, 2, 3, 4, DateTimeKind.Utc),
            FileName = "report.pdf",
            MimeType = "application/pdf",
            FileSize = 12345
        };

        S3MetadataResponseDto dto = S3MetadataResponseDtoMapper.MapS3MetadataToS3MetadataResponseDto(
            item,
            "https://example.test/file");

        Assert.Equal(item.Id, dto.Id);
        Assert.Equal(item.OwnerId, dto.OwnerId);
        Assert.Equal(item.Status, dto.Status);
        Assert.Equal(item.UploadedAt, dto.UploadedAt);
        Assert.Equal(item.LastRetrieved, dto.LastRetrieved);
        Assert.Equal(item.FileName, dto.FileName);
        Assert.Equal(item.MimeType, dto.MimeType);
        Assert.Equal(item.FileSize, dto.FileSize);
        Assert.Equal("https://example.test/file", dto.FileUrl);
    }
}