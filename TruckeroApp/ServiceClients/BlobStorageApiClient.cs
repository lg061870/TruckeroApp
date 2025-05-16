using System.Net.Http.Json;
using Truckero.Core.Interfaces;

public class BlobStorageApiClient : IBlobStorageService
{
    private readonly HttpClient _http;

    public BlobStorageApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry)
    {
        var response = await _http.PostAsJsonAsync("/api/blob/upload-url", new
        {
            FileName = blobName,
            ContainerName = containerName
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SasResponse>();
        return result?.SasUrl ?? throw new InvalidOperationException("SAS URL not returned.");
    }

    // These are not used on the client side, but required by the interface
    public Task<string> UploadAsync(Stream _, string __, string ___, string ____) =>
        throw new NotImplementedException();

    public Task<Stream?> DownloadAsync(string _, string __) =>
        throw new NotImplementedException();

    public Task DeleteAsync(string _, string __) =>
        throw new NotImplementedException();

    public Task<string> GetBlobUrl(string _, string __) =>
        throw new NotImplementedException();

    public Task<string> GenerateDownloadSasUrl(string _, string __, TimeSpan ___) =>
        throw new NotImplementedException();

    private class SasResponse
    {
        public string SasUrl { get; set; } = null!;
    }
}
