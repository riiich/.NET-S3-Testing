using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Interfaces
{
    public interface IS3FileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);

        string GetFileUrl(string s3Key);
    }
}
