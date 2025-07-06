using System.Text.Json;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;
using TruckeroApp.Models;

namespace TruckeroApp.Extensions; 
public static class PayoutAccountClientExtensions {
    // Mappings PayoutAccountRequest -> PayoutAccounFormModel --> PayoutAccountRequest
    public static PayoutAccountRequest ToPayoutAccountRequest(this PayoutAccountFormModel form) {
        return new PayoutAccountRequest {
            PaymentMethodTypeId = form.PaymentMethodTypeId,
            FullName = form.FullName,
            BankId = form.BankId,
            AccountNumberLast4 = form.AccountNumber,
            RoutingNumber = form.RoutingNumber,
            MobileNumber = form.MobileNumber,
            PayPalEmail = form.PayPalEmail,
            IsDefault = form.IsDefault,
            MetadataJson = form.MetadataJson // already built if you call BuildMetadataJson()
        };
    }
    public static PayoutAccountFormModel ToPayoutAccountFormModel(this PayoutAccountRequest req) {
        // Default values from metadata
        string? bankName = null, routingNumber = null, mobileNumber = null, payPalEmail = null;
        Guid? bankId = req.BankId;

        if (!string.IsNullOrWhiteSpace(req.MetadataJson)) {
            try {
                var meta = JsonSerializer.Deserialize<MetadataModel>(req.MetadataJson!);
                bankId = meta?.BankId ?? req.BankId;
                bankName = meta?.BankName;
                routingNumber = meta?.RoutingNumber;
                mobileNumber = meta?.MobileNumber;
                payPalEmail = meta?.PayPalEmail;
            } catch { /* ignore bad json */ }
        }

        return new PayoutAccountFormModel {
            // No Id (API request doesn't have it—set to Guid.Empty or handle in UI)
            PaymentMethodTypeId = req.PaymentMethodTypeId,
            FullName = req.FullName,
            BankId = bankId,
            BankName = bankName,
            AccountNumber = req.AccountNumberLast4,
            RoutingNumber = routingNumber,
            MobileNumber = mobileNumber,
            PayPalEmail = payPalEmail,
            IsDefault = req.IsDefault,
            MetadataJson = req.MetadataJson
        };
    }

    // Mappings PayoutAccount -> PayoutAccountFormModel -> PayoutAccount
    public static PayoutAccountFormModel ToPayoutAccountFormModel(this PayoutAccount acc) {
        // Default values
        string? bankName = null, routingNumber = null, mobileNumber = null, payPalEmail = null;
        Guid? bankId = null;

        if (!string.IsNullOrWhiteSpace(acc.MetadataJson)) {
            try {
                var meta = JsonSerializer.Deserialize<MetadataModel>(acc.MetadataJson!);
                bankId = meta?.BankId;
                bankName = meta?.BankName;
                routingNumber = meta?.RoutingNumber;
                mobileNumber = meta?.MobileNumber;
                payPalEmail = meta?.PayPalEmail;
            } catch { /* log error, ignore bad json */ }
        }

        return new PayoutAccountFormModel {
            Id = acc.Id,
            PaymentMethodTypeId = acc.PaymentMethodTypeId,
            FullName = acc.User?.FullName ?? "", // Or use other available name field
            BankId = bankId,
            BankName = bankName,
            AccountNumber = acc.AccountNumberLast4,
            RoutingNumber = routingNumber,
            MobileNumber = mobileNumber,
            PayPalEmail = payPalEmail,
            IsDefault = acc.IsDefault,
            MetadataJson = acc.MetadataJson
        };
    }
    public static PayoutAccount ToPayoutAccount(this PayoutAccountFormModel form, Guid userId) {
        return new PayoutAccount {
            Id = form.Id == Guid.Empty ? Guid.NewGuid() : form.Id,
            UserId = userId,
            PaymentMethodTypeId = form.PaymentMethodTypeId,
            AccountNumberLast4 = form.AccountNumber,
            IsDefault = form.IsDefault,
            MetadataJson = form.MetadataJson,
            // If you want to set CreatedAt, User, etc. handle those elsewhere
        };
    }

    // Define a class that matches your metadata structure
    private class MetadataModel {
        public Guid? BankId { get; set; }
        public string? BankName { get; set; }
        public string? RoutingNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? PayPalEmail { get; set; }
    }
}
