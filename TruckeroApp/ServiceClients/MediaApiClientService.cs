using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Media;
using TruckeroApp.Interfaces;

namespace TruckeroApp.ServiceClients
{
    /// <summary>
    /// Client service for interacting with the Media API endpoints
    /// </summary>
    public class MediaApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MediaApiClientService> _logger;
        private readonly IAuthSessionContext _sessionContext;

        public MediaApiClientService(HttpClient httpClient, ILogger<MediaApiClientService> logger, IAuthSessionContext sessionContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _sessionContext = sessionContext;
        }

        /// <summary>
        /// Uploads an image to the server
        /// </summary>
        /// <param name="fileStream">The image file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="fileType">Type of file (e.g., profile, license_front)</param>
        /// <returns>Response with URL if successful</returns>
        public async Task<ImageUploadResponse> UploadImageAsync(Stream fileStream, string fileName, string fileType)
        {
            try
            {
                // Ensure authentication token is set
                await EnsureAuthenticationHeaderAsync();

                // Create multipart form content
                using var content = new MultipartFormDataContent();
                
                // Add the file content
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
                content.Add(fileContent, "file", fileName);
                
                // Add the file type
                content.Add(new StringContent(fileType), "fileType");

                // Send the request
                var response = await _httpClient.PostAsync("api/Media/upload", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ImageUploadResponse>() 
                        ?? ImageUploadResponse.Failed("Failed to parse response");
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
                    return errorResponse ?? ImageUploadResponse.Failed($"API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return ImageUploadResponse.Failed($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an image by its file ID
        /// </summary>
        /// <param name="fileId">The ID of the file to delete</param>
        /// <returns>True if deletion was successful</returns>
        public async Task<bool> DeleteImageAsync(string fileId)
        {
            try
            {
                // Ensure authentication token is set
                await EnsureAuthenticationHeaderAsync();

                var response = await _httpClient.DeleteAsync($"api/Media/{fileId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// Gets a temporary URL for an image that expires after a specified time
        /// </summary>
        /// <param name="fileId">The ID of the file</param>
        /// <param name="expirationMinutes">Number of minutes until the URL expires</param>
        /// <returns>Temporary URL to access the image</returns>
        public async Task<string> GetTemporaryUrlAsync(string fileId, int expirationMinutes = 60)
        {
            try
            {
                // Ensure authentication token is set
                await EnsureAuthenticationHeaderAsync();

                var response = await _httpClient.GetAsync($"api/Media/url/{fileId}?expirationMinutes={expirationMinutes}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TemporaryUrlResponse>();
                    return result?.Url ?? string.Empty;
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting temporary URL: {FileId}", fileId);
                throw;
            }
        }

        /// <summary>
        /// Ensures the authentication header is set with the current token
        /// </summary>
        private Task EnsureAuthenticationHeaderAsync()
        {
            var token = _sessionContext.AccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the content type based on file extension
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }

    /// <summary>
    /// Helper class for temporary URL response
    /// </summary>
    internal class TemporaryUrlResponse
    {
        public string Url { get; set; } = string.Empty;
    }
}
