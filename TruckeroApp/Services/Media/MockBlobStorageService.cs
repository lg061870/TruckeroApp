using Microsoft.Extensions.Logging;
using Truckero.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TruckeroApp.Services.Media;

/// <summary>
/// Mock implementation of IBlobStorageService for development and testing.
/// Simulates basic file storage operations on local filesystem.
/// </summary>
public class MockBlobStorageService : IBlobStorageService
{
    private readonly ILogger<MockBlobStorageService> _logger;
    private readonly string _platformImageRoot;
    private readonly bool _isMaui;

    public MockBlobStorageService(ILogger<MockBlobStorageService> logger)
    {
        _logger = logger;
        // Detect if running on MAUI (mobile/desktop) or web/server
        // If FileSystem.AppDataDirectory exists, use it; otherwise, use wwwroot
        try
        {
            _platformImageRoot = FileSystem.AppDataDirectory;
            _isMaui = true;
        }
        catch
        {
            _platformImageRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            _isMaui = false;
        }
    }

    private string GetImageDirectory(string? userId = null)
    {
        return userId == null
            ? Path.Combine(_platformImageRoot, "images", "license")
            : Path.Combine(_platformImageRoot, "images", userId, "license");
    }

    private string GetImagePath(string fileName, string? userId = null)
    {
        return Path.Combine(GetImageDirectory(userId), fileName);
    }

    private string GetImageUrl(string fileName, string? userId = null)
    {
        if (_isMaui)
        {
            // On MAUI, return file:// URL for local preview
            var localPath = GetImagePath(fileName, userId);
            return $"file://{localPath}";
        }
        else
        {
            // On web/server, return relative URL for browser
            return userId == null
                ? $"/images/license/{fileName}"
                : $"/images/{userId}/license/{fileName}";
        }
    }

    public Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType)
        => UploadAsync(fileStream, fileName, containerName, contentType, null);

    public Task<Stream?> DownloadAsync(string blobName, string containerName)
        => DownloadAsync(blobName, containerName, null);

    public Task DeleteAsync(string blobName, string containerName)
        => DeleteAsync(blobName, containerName, null);

    public Task<string> GetBlobUrl(string blobName, string containerName)
        => GetBlobUrl(blobName, containerName, null);

    public Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry)
        => GenerateUploadSasUrl(blobName, containerName, expiry, null);

    public Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry)
        => GenerateDownloadSasUrl(blobName, containerName, expiry, null);

    // Overloads with userId (used by app logic)
    public Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType, string? userId)
    {
        try
        {
            var fullPath = GetImagePath(fileName, userId);

            // Ensure full directory exists, including subdirectories in fileName
            var targetDirectory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(targetDirectory);

            _logger.LogInformation("MockBlobStorage: Saving file to {FullPath}", fullPath);

            using var fileStreamOut = File.Create(fullPath);
            fileStream.CopyTo(fileStreamOut);

            var url = GetImageUrl(fileName, userId);
            return Task.FromResult(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MockBlobStorage: Failed to save file {FileName}", fileName);
            throw;
        }
    }

    public Task<Stream?> DownloadAsync(string blobName, string containerName, string? userId)
    {
        var path = GetImagePath(blobName, userId);
        _logger.LogInformation("MockBlobStorage: Downloading file from {Path}", path);

        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);

        var stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string blobName, string containerName, string? userId)
    {
        var path = GetImagePath(blobName, userId);
        _logger.LogInformation("MockBlobStorage: Deleting file {Path}", path);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<string> GetBlobUrl(string blobName, string containerName, string? userId)
    {
        var url = GetImageUrl(blobName, userId);
        _logger.LogInformation("MockBlobStorage: Returning local URL {Url}", url);
        return Task.FromResult(url);
    }

    public Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry, string? userId)
    {
        var url = GetImageUrl(blobName, userId);
        return Task.FromResult(url + "?sas=mock");
    }

    public Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry, string? userId)
    {
        var url = GetImageUrl(blobName, userId);
        return Task.FromResult(url + "?sas=mock");
    }
}
