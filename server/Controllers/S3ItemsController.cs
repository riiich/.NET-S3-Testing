using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/s3-items")]
public class S3ItemsController : ControllerBase
{
    private readonly IS3ItemService _s3ItemService;
    private readonly IS3FileStorageService _s3FileStorageService;

    public S3ItemsController(IS3ItemService s3ItemService, IS3FileStorageService s3FileStorageService)
    {
        _s3ItemService = s3ItemService;
        _s3FileStorageService = s3FileStorageService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        S3Item? item = await _s3ItemService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(item));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        IReadOnlyList<S3Item> items = await _s3ItemService.GetByUserIdAsync(userId);

        return Ok(items.Select(ToResponseDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateS3ItemRequest request)
    {
        try
        {
            S3Item item = await _s3ItemService.CreateAsync(request);

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
        S3Item? item = await _s3ItemService.MarkRetrievedAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(item));
    }

    private S3ItemResponseDto ToResponseDto(S3Item item)
    {
        return new S3ItemResponseDto
        {
            Id = item.Id,
            UserId = item.UserId,
            S3Key = item.S3Key,
            UploadedAt = item.UploadedAt,
            LastRetrieved = item.LastRetrieved,
            FileName = item.FileName,
            MimeType = item.MimeType,
            FileSize = item.FileSize,
            FileUrl = _s3FileStorageService.GetFileUrl(item.S3Key)
        };
    }
}
