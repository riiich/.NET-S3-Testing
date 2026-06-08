using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using server.Interfaces;
using server.Models;
using System.Text;

namespace server.Services
{
    public class S3FileUploadService : IS3FileUploadService
    {
        private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _s3Settings;
        private readonly UploadSettings _uploadSettings;

        public S3FileUploadService(IAmazonS3 s3Client, IOptions<S3Settings> s3SettingsOptions, IOptions<UploadSettings> uploadSettingsOptions)
        {
            _s3Client = s3Client;
            _s3Settings = s3SettingsOptions.Value;
            _uploadSettings = uploadSettingsOptions.Value;
        }

        public string CreateObjectKey(string fileName)
        {
            string safeFileName = Path.GetFileName(fileName);
            string extension = Path.GetExtension(safeFileName).ToLowerInvariant();

            return $"uploads/{Guid.NewGuid()}{extension}";
        }

        public async Task ValidateFileAsync(FileUploadInput file)
        {
            string extension = ValidateFile(file);
            await ValidateFileContentAsync(file.Content, extension);
        }

        public async Task UploadFileAsync(FileUploadInput file, string s3Key)
        {
            await ValidateFileAsync(file);
            ValidateS3Settings();
            
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = s3Key,
                    InputStream = file.Content,
                    ContentType = file.ContentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch (AmazonS3Exception e)
            {
                throw new InvalidOperationException("There was an error uploading to S3.", e);
            }
        }

        public async Task DeleteFileAsync(string s3Key)
        {
            ValidateS3Settings();

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = s3Key
            };

            await _s3Client.DeleteObjectAsync(request);
        }

        public string GetPresignedUrl(string s3Key)
        {
            ValidateS3Settings();

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }

        private string ValidateFile(FileUploadInput file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "A file is required.");
            }

            if (file.Length <= 0)
            {
                throw new ArgumentException("File is empty.");
            }

            if (file.Length > _uploadSettings.MaxFileSizeBytes)
            {
                throw new ArgumentException($"File exceeds the maximum allowed size of {_uploadSettings.MaxFileSizeBytes} bytes.");
            }

            if (!_uploadSettings.AcceptedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF and CSV files are allowed.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_uploadSettings.AcceptedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only files with .pdf and .csv extensions are allowed.");
            }

            return extension;
        }

        private static Task ValidateFileContentAsync(Stream stream, string extension)
        {
            return extension switch
            {
                ".pdf" => ValidatePdfSignatureAsync(stream),
                ".csv" => ValidateCsvContentAsync(stream),
                _ => throw new ArgumentException("Unsupported file extension.")
            };
        }

        private static async Task ValidatePdfSignatureAsync(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Uploaded file stream must be seekable for validation.");
            }

            long originalPosition = stream.Position;
            stream.Position = 0;

            byte[] buffer = new byte[PdfSignature.Length];
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));

            stream.Position = originalPosition;

            if (bytesRead != PdfSignature.Length || !buffer.SequenceEqual(PdfSignature))
            {
                throw new ArgumentException("Uploaded file is not a valid PDF.");
            }
        }

        private static async Task ValidateCsvContentAsync(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Uploaded file stream must be seekable for validation.");
            }

            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                using StreamReader reader = new(
                    stream,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true);

                char[] buffer = new char[4096];
                int charsRead = await reader.ReadBlockAsync(buffer.AsMemory(0, buffer.Length));

                if (charsRead == 0 || buffer.AsSpan(0, charsRead).Contains('\0'))
                {
                    throw new ArgumentException("Uploaded file is not a valid CSV.");
                }
            }
            catch (DecoderFallbackException e)
            {
                throw new ArgumentException("Uploaded file is not a valid CSV.", e);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private void ValidateS3Settings()
        {
            if (string.IsNullOrWhiteSpace(_s3Settings.BucketName))
            {
                throw new InvalidOperationException("S3 bucket name is not configured.");
            }
        }
    }
}
