using Truckero.Core.DTOs.Media;

namespace Truckero.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for handling media uploads and management
    /// </summary>
    public interface IMediaService
    {
        /// <summary>
        /// Uploads an image file and returns the URL
        /// </summary>
        /// <param name="request">The image upload request containing file type and validation parameters</param>
        /// <returns>Response with success status and image URL if successful</returns>
        Task<ImageUploadResponse> UploadImageAsync(ImageUploadRequest request);

        /// <summary>
        /// Deletes an image by its file ID
        /// </summary>
        /// <param name="fileId">The ID of the file to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteImageAsync(string fileId);

        /// <summary>
        /// Gets a temporary URL for an image that expires after a specified time
        /// </summary>
        /// <param name="fileId">The ID of the file</param>
        /// <param name="expirationMinutes">Number of minutes until the URL expires</param>
        /// <returns>Temporary URL to access the image</returns>
        Task<string> GetTemporaryUrlAsync(string fileId, int expirationMinutes = 60);
    }
}
