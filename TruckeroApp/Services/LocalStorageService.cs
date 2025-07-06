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
        } catch (JsonException js) {
            Console.WriteLine($"Failed to deserialize '{key}' from localStorage. Attempting migration...");

            try {
                // Parse as generic JSON document for migration
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var obj = new Dictionary<string, object?>();

                foreach (var prop in root.EnumerateObject()) {
                    // Trucks[].UseTags → Trucks[].UseTagIds migration
                    if (prop.Name == "Trucks" && prop.Value.ValueKind == JsonValueKind.Array) {
                        var trucks = new List<Dictionary<string, object?>>();

                        foreach (var truckElem in prop.Value.EnumerateArray()) {
                            var truckDict = new Dictionary<string, object?>();
                            foreach (var tProp in truckElem.EnumerateObject()) {
                                if (tProp.Name == "UseTags" && tProp.Value.ValueKind == JsonValueKind.Array) {
                                    var useTagIds = tProp.Value.EnumerateArray()
                                        .Select(tagObj =>
                                            tagObj.TryGetProperty("UseTagId", out var idProp)
                                                ? idProp.GetGuid()
                                                : Guid.Empty)
                                        .Where(guid => guid != Guid.Empty)
                                        .ToList();
                                    truckDict["UseTagIds"] = useTagIds;
                                }
                                else if (tProp.Name != "UseTags") {
                                    truckDict[tProp.Name] = tProp.Value.Deserialize<object?>(_jsonOptions);
                                }
                            }
                            trucks.Add(truckDict);
                        }

                        obj["Trucks"] = trucks;
                    }
                    // 🟠 PATCH: DriverProfileRequest.CountryCode migration for missing property
                    else if (prop.Name == "Profile" && prop.Value.ValueKind == JsonValueKind.Object) {
                        var profileDict = new Dictionary<string, object?>();

                        foreach (var pProp in prop.Value.EnumerateObject())
                            profileDict[pProp.Name] = pProp.Value.Deserialize<object?>(_jsonOptions);

                        // Add CountryCode if missing (default to empty string for now, fix in UI)
                        if (!profileDict.ContainsKey("CountryCode"))
                            profileDict["CountryCode"] = "CR";

                        obj["Profile"] = profileDict;
                    }
                    // Default: Copy as-is
                    else {
                        obj[prop.Name] = prop.Value.Deserialize<object?>(_jsonOptions);
                    }
                }

                // Serialize back to patched JSON and retry deserialization
                var fixedJson = JsonSerializer.Serialize(obj, _jsonOptions);

                if (fixedJson.Length > 4_000_000)
                    Console.WriteLine("Warning: fixedJson is very large (>4MB) and may exceed localStorage quota.");

                try {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, fixedJson);
                } catch (Exception jsEx) {
                    Console.WriteLine($"localStorage.setItem threw: {jsEx.Message}");
                    Console.WriteLine($"fixedJson length: {fixedJson?.Length}");
                    // Optionally: log fixedJson or part of it
                    // You can also check for JSException and look at its details if using Blazor
                }


                // Try to deserialize again with fixed json
                return JsonSerializer.Deserialize<T>(fixedJson, _jsonOptions);
            } catch (Exception ex2) {
                Console.WriteLine($"Migration failed: {ex2.Message}");
                return default;
            }
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