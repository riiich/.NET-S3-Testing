using Amazon.S3;
using Microsoft.Extensions.Options;
using server.Models;
using Amazon.S3.Model;
using server.Interfaces;

namespace server.Services
{
    public class S3FileStorageService : IS3FileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _s3Settings;
        private readonly UploadSettings _uploadSettings;

        public S3FileStorageService(IAmazonS3 s3Client, IOptions<S3Settings> s3SettingsOptions, IOptions<UploadSettings> uploadSettingsOptions)
        {
            _s3Client = s3Client;
            _s3Settings = s3SettingsOptions.Value;
            _uploadSettings = uploadSettingsOptions.Value;
        }

        public string CreateObjectKey(string fileName)
        {
            string safeFileName = Path.GetFileName(fileName);

            return $"uploads/{Guid.NewGuid()}-{safeFileName}";
        }

        public async Task UploadFileAsync(FileUploadInput file, string s3Key)
        {
            if(file == null)
            {
                throw new ArgumentNullException("There was an error uploading the file!");
            }

            if(file.Length <= 0)
            {
                throw new ArgumentException("File is empty...");
            }

            if(!_uploadSettings.AcceptedTypes.Contains(file.ContentType))
            {
                throw new ArgumentException ("Invalid file type was uploaded!");
            }
            
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName =  _s3Settings.BucketName,
                    Key = s3Key,
                    InputStream = file.Content,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch(AmazonS3Exception e)
            {
                throw new AmazonS3Exception("There was an error uploading to S3 =( ...)", e);
            }
        }

        public async Task DeleteFileAsync(string s3Key)
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = s3Key
            };

            await _s3Client.DeleteObjectAsync(request);
        }

        public string GetFileUrl(string s3Key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
