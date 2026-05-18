using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("/api/s3")]
    public class S3Controller : ControllerBase
    {
        private readonly IS3FileStorageService _s3Service;
        private readonly IS3ItemService _s3ItemService;
        private readonly IUserService _userService;

        public S3Controller(
            IS3FileStorageService s3Service,
            IS3ItemService s3ItemService,
            IUserService userService)
        {
            _s3Service = s3Service;
            _s3ItemService = s3ItemService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult test()
        {
            return Ok(new { msg = "Made it to the S3 bucket upload root endpoint!" });
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> FileUpload(IFormFile uploadedFile, [FromForm] int userId)
        {
            try
            {
                User? user = await _userService.GetByIdAsync(userId);

                if (user is null)
                {
                    return BadRequest("Create an account before uploading files.");
                }

                var response = await _s3Service.UploadFileAsync(uploadedFile, uploadedFile.FileName);
                S3Item item = await _s3ItemService.CreateAsync(new CreateS3ItemRequest
                {
                    UserId = userId,
                    S3Key = response,
                    FileName = uploadedFile.FileName,
                    MimeType = uploadedFile.ContentType,
                    FileSize = uploadedFile.Length
                });

                return Ok(new { msg="File uploaded successfully =)", res=response, item });
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
