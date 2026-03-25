using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace SmartInsuranceHub.Services
{
    /// <summary>
    /// Handles all file operations with Cloudflare R2 (S3-compatible).
    /// Reads credentials from environment variables.
    /// </summary>
    public class R2StorageService
    {
        private readonly ILogger<R2StorageService> _logger;
        private readonly string? _accessKey;
        private readonly string? _secretKey;
        private readonly string? _bucket;
        private readonly string? _endpoint;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/jpg",
            "image/png"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png"
        };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_accessKey)
                                    && !string.IsNullOrWhiteSpace(_secretKey)
                                    && !string.IsNullOrWhiteSpace(_bucket)
                                    && !string.IsNullOrWhiteSpace(_endpoint);

        public R2StorageService(ILogger<R2StorageService> logger)
        {
            _logger = logger;
            _accessKey = Environment.GetEnvironmentVariable("CLOUDFLARE_R2_ACCESS_KEY");
            _secretKey = Environment.GetEnvironmentVariable("CLOUDFLARE_R2_SECRET_KEY");
            _bucket = Environment.GetEnvironmentVariable("CLOUDFLARE_R2_BUCKET");
            _endpoint = Environment.GetEnvironmentVariable("CLOUDFLARE_R2_ENDPOINT");
        }

        private AmazonS3Client CreateClient()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _endpoint?.TrimEnd('/'),
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            };
            return new AmazonS3Client(_accessKey, _secretKey, config);
        }

        /// <summary>
        /// Validates the uploaded file before processing.
        /// </summary>
        public (bool IsValid, string Error) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "No file selected.");

            if (file.Length > MaxFileSize)
                return (false, "File size exceeds the 10MB limit.");

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                return (false, "Invalid file type. Only PDF, JPG, and PNG are allowed.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return (false, "Invalid content type. Only PDF, JPG, and PNG are allowed.");

            return (true, "");
        }

        /// <summary>
        /// Uploads a file to Cloudflare R2.
        /// Returns the object key on success, or null on failure.
        /// </summary>
        public async Task<(string? FileUrl, string? Error)> UploadFileAsync(IFormFile file, string userType, int userId, string category)
        {
            if (!IsConfigured)
            {
                _logger.LogError("Cloudflare R2 is not configured. Please set CLOUDFLARE_R2_ACCESS_KEY, CLOUDFLARE_R2_SECRET_KEY, CLOUDFLARE_R2_BUCKET, and CLOUDFLARE_R2_ENDPOINT environment variables in your .env file.");
                return (null, "Cloudflare R2 is not configured. Please check environment variables.");
            }

            try
            {
                var extension = Path.GetExtension(file.FileName);
                var objectKey = $"documents/{userType.ToLower()}/{userId}/{category.Replace(" ", "_").ToLower()}/{Guid.NewGuid()}{extension}";

                using var client = CreateClient();
                using var stream = file.OpenReadStream();

                var request = new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    DisablePayloadSigning = true
                };

                await client.PutObjectAsync(request);
                _logger.LogInformation("Uploaded {FileName} to R2 as {Key}", file.FileName, objectKey);
                return (objectKey, null);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 Exception while uploading to R2.");
                return (null, $"Storage Error [{ex.ErrorCode}]: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Cloudflare R2.");
                return (null, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a presigned URL for viewing a document (valid for 1 hour).
        /// </summary>
        public async Task<string?> GetPresignedUrlAsync(string objectKey)
        {
            if (string.IsNullOrEmpty(objectKey)) return null;

            // Old local fallback URLs start with '/' — these files only exist on the PC that uploaded them.
            // They cannot be served across PCs. The document must be re-uploaded to R2.
            if (objectKey.StartsWith("/"))
            {
                _logger.LogWarning("Document has a local fallback URL ({Key}). It must be re-uploaded to Cloudflare R2 to be viewable across PCs.", objectKey);
                return null;
            }

            if (!IsConfigured)
            {
                _logger.LogError("R2 is not configured. Cannot generate presigned URL.");
                return null;
            }

            try
            {
                using var client = CreateClient();
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucket,
                    Key = objectKey,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                return await Task.FromResult(client.GetPreSignedURL(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate presigned URL for {Key}", objectKey);
                return null;
            }
        }

        /// <summary>
        /// Deletes a file from R2.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string objectKey)
        {
            if (string.IsNullOrEmpty(objectKey)) return false;

            // Old local fallback URLs — nothing to delete from R2
            if (objectKey.StartsWith("/"))
            {
                _logger.LogWarning("Skipping delete for local fallback URL: {Key}", objectKey);
                return true;
            }

            if (!IsConfigured)
            {
                _logger.LogError("R2 is not configured. Cannot delete file.");
                return false;
            }

            try
            {
                using var client = CreateClient();
                await client.DeleteObjectAsync(_bucket, objectKey);
                _logger.LogInformation("Deleted {Key} from R2.", objectKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {Key} from R2.", objectKey);
                return false;
            }
        }
    }
}
