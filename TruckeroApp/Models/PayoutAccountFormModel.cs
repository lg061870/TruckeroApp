using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace TruckeroApp.Models;

public class PayoutAccountFormModel
{
    public Guid Id { get; set; }
    [Required]
    public Guid PaymentMethodTypeId { get; set; }
    [Required]
    public string FullName { get; set; } = string.Empty;
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? ConfirmAccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? PayPalEmail { get; set; }
    public string? ConfirmPayPalEmail { get; set; }
    public bool IsDefault { get; set; }
    // For extensibility, store extra fields as JSON
    public string? MetadataJson { get; set; }
    public void BuildMetadataJson()
    {
        var meta = new {
            BankId,
            BankName,
            RoutingNumber,
            MobileNumber,
            PayPalEmail
        };
        MetadataJson = JsonSerializer.Serialize(meta);
    }
}