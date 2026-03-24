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
        public async Task<string?> UploadFileAsync(IFormFile file, string userType, int userId, string category)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("R2 is not configured. File saved locally as fallback.");
                return await SaveLocallyAsync(file, userType, userId, category);
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
                    ContentType = file.ContentType
                };

                await client.PutObjectAsync(request);
                _logger.LogInformation("Uploaded {FileName} to R2 as {Key}", file.FileName, objectKey);
                return objectKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload to R2. Using local fallback.");
                return await SaveLocallyAsync(file, userType, userId, category);
            }
        }

        /// <summary>
        /// Generates a presigned URL for viewing a document (valid for 1 hour).
        /// </summary>
        public async Task<string?> GetPresignedUrlAsync(string objectKey)
        {
            if (string.IsNullOrEmpty(objectKey)) return null;

            // If the key starts with '/', it was saved via the local fallback.
            if (objectKey.StartsWith("/"))
                return GetLocalUrl(objectKey);

            if (!IsConfigured)
                return GetLocalUrl(objectKey);

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

            // If the key starts with '/', it was saved via the local fallback.
            if (objectKey.StartsWith("/"))
                return DeleteLocal(objectKey);

            if (!IsConfigured)
                return DeleteLocal(objectKey);

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

        // ============================
        // Local fallback (for development without R2)
        // ============================
        private async Task<string?> SaveLocallyAsync(IFormFile file, string userType, int userId, string category)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName);
                var folder = Path.Combine("wwwroot", "uploads", userType.ToLower(), userId.ToString(), category.Replace(" ", "_").ToLower());
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var relativeUrl = $"/uploads/{userType.ToLower()}/{userId}/{category.Replace(" ", "_").ToLower()}/{fileName}";
                _logger.LogInformation("Saved {FileName} locally at {Path}", file.FileName, relativeUrl);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file locally.");
                return null;
            }
        }

        private string? GetLocalUrl(string? key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return key.StartsWith("/") ? key : null;
        }

        private bool DeleteLocal(string? key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith("/")) return false;
            var path = Path.Combine("wwwroot", key.TrimStart('/'));
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
    }
}
