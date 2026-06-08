using server.DTOs;
using server.Models;

namespace server.Mappers;

public static class S3MetadataResponseDtoMapper
{
    public static S3MetadataResponseDto MapS3MetadataToS3MetadataResponseDto(S3Metadata item, string fileUrl)
    {
        return new S3MetadataResponseDto
        {
            Id = item.Id,
            OwnerId = item.OwnerId,
            Status = item.Status,
            UploadedAt = item.UploadedAt,
            LastRetrieved = item.LastRetrieved,
            FileName = item.FileName,
            MimeType = item.MimeType,
            FileSize = item.FileSize,
            FileUrl = fileUrl
        };
    }
}