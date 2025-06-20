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

    public async Task<T?> GetItemAsync<T>(string key) {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
            return default;

        try {
            // Try direct deserialize
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        } catch (JsonException) {
            Console.WriteLine($"Failed to deserialize '{key}' from localStorage. Attempting migration...");

            try {
                // Parse as generic JSON document to fix the UseTags->UseTagIds migration
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Copy all properties, but fix Trucks[].UseTags -> Trucks[].UseTagIds
                var obj = new Dictionary<string, object?>();

                foreach (var prop in root.EnumerateObject()) {
                    if (prop.Name == "Trucks" && prop.Value.ValueKind == JsonValueKind.Array) {
                        var trucks = new List<Dictionary<string, object?>>();

                        foreach (var truckElem in prop.Value.EnumerateArray()) {
                            var truckDict = new Dictionary<string, object?>();
                            foreach (var tProp in truckElem.EnumerateObject()) {
                                if (tProp.Name == "UseTags" && tProp.Value.ValueKind == JsonValueKind.Array) {
                                    // Map to UseTagIds
                                    var useTagIds = tProp.Value.EnumerateArray()
                                        .Select(tagObj =>
                                            tagObj.TryGetProperty("UseTagId", out var idProp)
                                                ? idProp.GetGuid()
                                                : Guid.Empty
                                        ).Where(guid => guid != Guid.Empty)
                                         .ToList();
                                    truckDict["UseTagIds"] = useTagIds;
                                }
                                else if (tProp.Name != "UseTags") {
                                    // Copy property as-is
                                    truckDict[tProp.Name] = tProp.Value.Deserialize<object?>(_jsonOptions);
                                }
                            }
                            trucks.Add(truckDict);
                        }

                        obj["Trucks"] = trucks;
                    }
                    else {
                        obj[prop.Name] = prop.Value.Deserialize<object?>(_jsonOptions);
                    }
                }

                // Serialize back to fixed JSON and retry deserialization
                var fixedJson = JsonSerializer.Serialize(obj, _jsonOptions);
                // Optionally store the fixed version
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, fixedJson);

                // Try to deserialize again
                return JsonSerializer.Deserialize<T>(fixedJson, _jsonOptions);
            } catch (Exception ex2) {
                Console.WriteLine($"Migration failed: {ex2.Message}");
                return default;
            }
        }
    }

    //public async Task<T?> GetItemAsync<T>(string key)
    //{
    //    var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

    //    if (string.IsNullOrEmpty(json))
    //        return default;

    //    try
    //    {
    //        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    //    }
    //    catch (JsonException)
    //    {
    //        // If we can't deserialize (format changed, etc.), return default
    //        Console.WriteLine($"Failed to deserialize '{key}' from localStorage");
    //        return default;
    //    }
    //}

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