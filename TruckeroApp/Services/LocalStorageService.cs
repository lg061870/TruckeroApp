using Microsoft.JSInterop;
using System.Text.Json;
using TruckeroApp.Interfaces;

namespace TruckeroApp.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true // Makes saved data more readable for debugging
    };

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        
        if (string.IsNullOrEmpty(json))
            return default;
        
        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            // If we can't deserialize (format changed, etc.), return default
            Console.WriteLine($"Failed to deserialize '{key}' from localStorage");
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        if (value == null)
        {
            await RemoveItemAsync(key);
            return;
        }

        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.clear");
    }

    public async Task<bool> ContainKeyAsync(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        return !string.IsNullOrEmpty(json);
    }
}