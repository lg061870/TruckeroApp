using System;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Onboarding;

public class PayoutAccountResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public PayoutAccount? PayoutAccount { get; set; }
    public string? ErrorCode { get; set; }
}
