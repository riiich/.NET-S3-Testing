using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("/api/s3")]
    public class S3Controller : ControllerBase
    {
        private readonly IFileUploadWorkflowService _fileUploadWorkflowService;

        public S3Controller(IFileUploadWorkflowService fileUploadWorkflowService)
        {
            _fileUploadWorkflowService = fileUploadWorkflowService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> FileUpload(IFormFile uploadedFile, [FromForm] int userId)
        {
            try
            {
                if (uploadedFile is null)
                {
                    return BadRequest("A file is required.");
                }

                await using Stream fileContent = uploadedFile.OpenReadStream();
                StoredS3File item = await _fileUploadWorkflowService.UploadAsync(userId, new FileUploadInput
                {
                    FileName = uploadedFile.FileName,
                    ContentType = uploadedFile.ContentType,
                    Length = uploadedFile.Length,
                    Content = fileContent
                });

                return Ok(new { msg="File uploaded successfully =)", res=item.S3Key, item });
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch(InvalidOperationException e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
