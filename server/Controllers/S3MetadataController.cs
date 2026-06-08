using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;
using server.DTOs;
using server.Mappers;

namespace server.Controllers;

[ApiController]
[Route("api/s3-metadata")]
public class S3MetadataController : ControllerBase
{
    private readonly IS3MetadataService _s3MetadataService;
    private readonly IS3FileUploadService _s3FileUploadService;

    public S3MetadataController(IS3MetadataService s3MetadataService, IS3FileUploadService s3FileUploadService)
    {
        _s3MetadataService = s3MetadataService;
        _s3FileUploadService = s3FileUploadService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        S3Metadata? item = await _s3MetadataService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(MapS3MetadataToResponseDto(item));
    }

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwnerId(string ownerId)
    {
        IReadOnlyList<S3Metadata> items = await _s3MetadataService.GetByOwnerIdAsync(ownerId);

        if (items.Count == 0)
        {
            return NotFound("No S3 metadata exists for this owner.");
        }

        return Ok(items.Select(MapS3MetadataToResponseDto));
    }

    [HttpGet("owner/{ownerId}/{s3MetadataId:int}/file")]
    public async Task<IActionResult> RetrieveFile([FromRoute] string ownerId, [FromRoute] int s3MetadataId)
    {
        S3Metadata? item = await _s3MetadataService.GetByOwnerIdAndIdAsync(ownerId, s3MetadataId);

        if (item is null)
        {
            return NotFound();
        }

        S3Metadata? retrievedItem = await _s3MetadataService.MarkRetrievedAsync(item.Id);

        if (retrievedItem is null)
        {
            return NotFound();
        }

        string presignedUrl = _s3FileUploadService.GetPresignedUrl(item.S3Key);

        return Redirect(presignedUrl);
    }

    [HttpPatch("{id:int}/retrieved")]
    public async Task<IActionResult> MarkRetrieved(int id)
    {
        S3Metadata? item = await _s3MetadataService.MarkRetrievedAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(MapS3MetadataToResponseDto(item));
    }

    [HttpDelete("owner/{ownerId}/{s3MetadataId:int}")]
    public async Task<IActionResult> Delete([FromRoute] string ownerId, [FromRoute] int s3MetadataId)
    {
        S3Metadata? item = await _s3MetadataService.GetByOwnerIdAndIdAsync(ownerId, s3MetadataId);

        if (item is null)
        {
            return BadRequest("S3 metadata does not exist for this owner.");
        }

        await _s3FileUploadService.DeleteFileAsync(item.S3Key);
        await _s3MetadataService.DeleteAsync(item);

        return NoContent();
    }

    private S3MetadataResponseDto MapS3MetadataToResponseDto(S3Metadata item)
    {
        string fileUrl = Url.Action(
            nameof(RetrieveFile),
            null,
            new { ownerId = item.OwnerId, s3MetadataId = item.Id },
            Request.Scheme) ?? string.Empty;

        return S3MetadataResponseDtoMapper.MapS3MetadataToS3MetadataResponseDto(item, fileUrl);
    }
}