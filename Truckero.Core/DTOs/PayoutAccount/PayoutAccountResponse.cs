using System;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.PayoutAccount;

public class PayoutAccountResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PayoutAccountRequest> PayoutAccounts { get; set; } = new();
    public string? ErrorCode { get; set; }
}
