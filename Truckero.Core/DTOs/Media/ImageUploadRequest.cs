using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Media
{
    /// <summary>
    /// DTO for image upload requests
    /// </summary>
    public class ImageUploadRequest
    {
        /// <summary>
        /// The file stream containing the image data
        /// </summary>
        [Required]
        public Stream? FileStream { get; set; }

        /// <summary>
        /// Original filename of the uploaded file
        /// </summary>
        [Required]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Type of file being uploaded (e.g., "profile", "license_front", "license_back", "truck")
        /// </summary>
        [Required]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Maximum allowed file size in bytes
        /// </summary>
        public long MaxSizeBytes { get; set; } = 2 * 1024 * 1024; // Default 2MB

        /// <summary>
        /// Allowed file extensions (e.g., ".jpg", ".png")
        /// </summary>
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png" };

        /// <summary>
        /// Optional metadata to associate with the image
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Response DTO for image upload operations
    /// </summary>
    public class ImageUploadResponse
    {
        /// <summary>
        /// Indicates if the upload was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// URL of the uploaded image (if successful)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Error message (if unsuccessful)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Unique identifier for the uploaded file
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static ImageUploadResponse Succeeded(string url, string fileId)
        {
            return new ImageUploadResponse
            {
                Success = true,
                Url = url,
                FileId = fileId
            };
        }

        /// <summary>
        /// Creates a failed response with an error message
        /// </summary>
        public static ImageUploadResponse Failed(string errorMessage)
        {
            return new ImageUploadResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
