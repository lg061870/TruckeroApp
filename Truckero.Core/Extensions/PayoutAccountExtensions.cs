using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;

namespace Truckero.Core.Extensions; 
public static class PayoutAccountExtensions {
    public static PayoutAccountRequest ToPayoutAccountRequest(this PayoutAccount account) {
        return new PayoutAccountRequest {
            // No Id field in PayoutAccountRequest, intentionally omitted
            PaymentMethodTypeId = account.PaymentMethodTypeId,
            FullName = account.User?.FullName ?? string.Empty, // fallback for null User
            BankId = TryParseGuidFromMetadata(account.MetadataJson, "BankId"),
            AccountNumberLast4 = account.AccountNumberLast4,
            RoutingNumber = GetStringFromMetadata(account.MetadataJson, "RoutingNumber"),
            MobileNumber = GetStringFromMetadata(account.MetadataJson, "MobileNumber"),
            PayPalEmail = GetStringFromMetadata(account.MetadataJson, "PayPalEmail"),
            IsDefault = account.IsDefault,
            MetadataJson = account.MetadataJson
        };
    }

    // --- Helpers for extracting from MetadataJson ---
    private static Guid? TryParseGuidFromMetadata(string? metadataJson, string propertyName) {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;
        try {
            using var doc = System.Text.Json.JsonDocument.Parse(metadataJson);
            if (doc.RootElement.TryGetProperty(propertyName, out var val) && val.ValueKind == System.Text.Json.JsonValueKind.String)
                return Guid.TryParse(val.GetString(), out var guidVal) ? guidVal : (Guid?)null;
        } catch { }
        return null;
    }

    private static string? GetStringFromMetadata(string? metadataJson, string propertyName) {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;
        try {
            using var doc = System.Text.Json.JsonDocument.Parse(metadataJson);
            if (doc.RootElement.TryGetProperty(propertyName, out var val) && val.ValueKind == System.Text.Json.JsonValueKind.String)
                return val.GetString();
        } catch { }
        return null;
    }
}
