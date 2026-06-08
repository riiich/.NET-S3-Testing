using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Interfaces;
using server.Mappers;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("/api/s3")]
    public class S3Controller : ControllerBase
    {
        private readonly IFileUploadWorkflowService _fileUploadWorkflowService;
        private readonly LinkGenerator _linkGenerator;

        public S3Controller(IFileUploadWorkflowService fileUploadWorkflowService, LinkGenerator linkGenerator)
        {
            _fileUploadWorkflowService = fileUploadWorkflowService;
            _linkGenerator = linkGenerator;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> FileUpload(IFormFile uploadedFile, [FromForm] string ownerId)
        {
            try
            {
                if (uploadedFile is null)
                {
                    return BadRequest("A file is required.");
                }

                await using Stream fileContent = uploadedFile.OpenReadStream();
                S3Metadata item = await _fileUploadWorkflowService.UploadAsync(ownerId, new FileUploadInput
                {
                    FileName = uploadedFile.FileName,
                    ContentType = uploadedFile.ContentType,
                    Length = uploadedFile.Length,
                    Content = fileContent
                });

                string fileUrl = _linkGenerator.GetUriByAction(
                    HttpContext,
                    action: nameof(S3MetadataController.RetrieveFile),
                    controller: "S3Metadata",
                    values: new { ownerId = item.OwnerId, s3MetadataId = item.Id }) ?? string.Empty;

                S3MetadataResponseDto response = S3MetadataResponseDtoMapper.MapS3MetadataToS3MetadataResponseDto(item, fileUrl);

                return Ok(new { message = "File uploaded successfully.", item = response });
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}