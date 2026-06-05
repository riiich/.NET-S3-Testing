using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/s3-items")]
public class StoredS3FilesController : ControllerBase
{
    private readonly IStoredS3FileService _storedS3FileService;
    private readonly IS3FileUploadService _s3FileUploadService;

    public StoredS3FilesController(IStoredS3FileService storedS3FileService, IS3FileUploadService s3FileUploadService)
    {
        _storedS3FileService = storedS3FileService;
        _s3FileUploadService = s3FileUploadService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        StoredS3File? item = await _storedS3FileService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(item));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        IReadOnlyList<StoredS3File> items = await _storedS3FileService.GetByUserIdAsync(userId);

        return Ok(items.Select(ToResponseDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStoredS3FileRequest request)
    {
        try
        {
            StoredS3File item = await _storedS3FileService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponseDto(item));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPatch("{id:int}/retrieved")]
    public async Task<IActionResult> MarkRetrieved(int id)
    {
        StoredS3File? item = await _storedS3FileService.MarkRetrievedAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(item));
    }

    [HttpDelete("users/{userId:int}/{storedS3FileId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int userId, [FromRoute] int storedS3FileId)
    {
        StoredS3File? item = await _storedS3FileService.DeleteById(userId, storedS3FileId);

        if(item is null) return BadRequest("File does not exist in S3...");

        await _s3FileUploadService.DeleteFileAsync(item.S3Key);

        return NoContent();
    }

    private StoredS3FileResponseDto ToResponseDto(StoredS3File item)
    {
        return new StoredS3FileResponseDto
        {
            Id = item.Id,
            UserId = item.UserId,
            S3Key = item.S3Key,
            Status = item.Status,
            UploadedAt = item.UploadedAt,
            LastRetrieved = item.LastRetrieved,
            FileName = item.FileName,
            MimeType = item.MimeType,
            FileSize = item.FileSize,
            PresignedUrl = _s3FileUploadService.GetPresignedUrl(item.S3Key)
        };
    }
}
