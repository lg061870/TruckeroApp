using System;
using System.Collections.Generic;
using System.Text.Json;
using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock customer data objects for unit tests
/// </summary>
public static class MockCustomerTestData
{
    // Common IDs
    public static class Ids
    {
        public static readonly Guid ValidCustomerId = MockDriverTestData.Ids.ValidCustomerId;
        public static readonly Guid ValidUserId = Guid.Parse("cf2b3a2d-9b8c-4a43-a64f-ab56cd71de35");
        public static readonly Guid DefaultPaymentMethodId = Guid.Parse("d8902347-5d63-48bf-92a1-444c76a39016");
        // Use seeded IDs from AppDbContext for Card and Bank
        public static readonly Guid CardPaymentMethodTypeId = Guid.Parse("00000000-0000-0000-0000-000000000301");
        public static readonly Guid BankPaymentMethodTypeId = Guid.Parse("00000000-0000-0000-0000-000000000304");
    }

    // Payment Method Types
    public static PaymentMethodType CardPaymentMethodType => new()
    {
        Id = Ids.CardPaymentMethodTypeId,
        Name = "Card",
        Description = "Credit or debit card",
        IsForPayment = true,
        IsForPayout = false
    };

    public static PaymentMethodType BankPaymentMethodType => new()
    {
        Id = Ids.BankPaymentMethodTypeId,
        Name = "Bank",
        Description = "Bank account",
        IsForPayment = true,
        IsForPayout = true
    };

    // Sample customer profiles
    public static CustomerProfile StandardCustomer => new()
    {
        Id = Ids.ValidCustomerId,
        UserId = Ids.ValidUserId,
        CreatedAt = DateTime.UtcNow.AddMonths(-3)
    };

    public static CustomerProfile NewCustomer => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow.AddDays(-1)
    };

    // Helper to create a customer with payment methods
    public static CustomerProfile CreateCustomerWithPaymentMethods()
    {
        var customer = new CustomerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            PaymentAccounts = new List<PaymentAccount>()
        };

        // Add payment methods to this customer
        var creditCard = new PaymentAccount
        {
            Id = Ids.DefaultPaymentMethodId,
            UserId = customer.UserId,
            PaymentMethodTypeId = Ids.CardPaymentMethodTypeId,
            IsDefault = true,
            TokenizedId = "tok_visa_1234",
            AccountNumberLast4 = "1234",
            CreatedAt = customer.CreatedAt,
            MetadataJson = JsonSerializer.Serialize(new CardMetadata
            {
                Brand = "Visa",
                ExpiryMonth = "12",
                ExpiryYear = (DateTime.UtcNow.Year + 3).ToString(),
                Country = "US"
            })
        };

        var bankAccount = new PaymentAccount
        {
            Id = Guid.NewGuid(),
            UserId = customer.UserId,
            PaymentMethodTypeId = Ids.BankPaymentMethodTypeId,
            IsDefault = false,
            TokenizedId = "ba_checking_5678",
            AccountNumberLast4 = "5678",
            CreatedAt = customer.CreatedAt.AddDays(1),
            MetadataJson = JsonSerializer.Serialize(new BankMetadata
            {
                AccountType = "Checking",
                BankName = "Demo Bank"
            })
        };

        customer.PaymentAccounts.Add(creditCard);
        customer.PaymentAccounts.Add(bankAccount);

        return customer;
    }

    // User associated with customer - reference the existing one from MockUserTestData
    public static User CustomerUser => MockUserTestData.CustomerUser;
}

// Additional metadata classes needed for testing
public class BankMetadata
{
    public string AccountType { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}