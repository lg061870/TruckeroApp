using Microsoft.AspNetCore.Mvc;
using Truckero.API.Models;
using Truckero.Core.Interfaces;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlobStorageController : ControllerBase
{
    private readonly IBlobStorageService _blobStorage;

    public BlobStorageController(IBlobStorageService blobStorage)
    {
        _blobStorage = blobStorage;
    }

    /// <summary>
    /// Returns a temporary upload URL (SAS token) for the client to upload directly to Azure Blob Storage.
    /// </summary>
    [HttpPost("upload-url")]
    public async Task<ActionResult<BlobSasUrlResponse>> GetUploadUrl([FromBody] BlobUploadRequest request)
    {
        // 🚧 TODOs for future hardening:
        // - Validate `request.ContainerName` against allowed values (e.g., "profiles", "documents")
        // - Optionally validate/normalize file extension (e.g., .jpg, .pdf only)
        // - Log upload intent or associate with current user (if authenticated)
        // - Rate limit or throttle abuse (e.g., too many uploads in short time)

        var sasUrl = await _blobStorage.GenerateUploadSasUrl(
            request.FileName,
            request.ContainerName,
            TimeSpan.FromMinutes(15)
        );

        return Ok(new BlobSasUrlResponse { SasUrl = sasUrl });
    }
}
