using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders.Testing;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("/api/s3")]
    public class S3Controller : ControllerBase
    {
        private readonly S3FileStorageService _s3Service;

        public S3Controller(S3FileStorageService s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpGet]
        public IActionResult test()
        {
            return Ok(new { msg = "Made it to the S3 bucket upload root endpoint!" });
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> FileUpload(IFormFile uploadedFile)
        {
            try
            {
                var response = await _s3Service.UploadFileAsync(uploadedFile, uploadedFile.FileName);

                return Ok(new { msg="File uploaded successfully =)", res=response });
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