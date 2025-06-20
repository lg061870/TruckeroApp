namespace Truckero.Core.Extensions; 
public static class PaymentMethodTypeExtensions {
    public static string GetNameById(this IEnumerable<PaymentMethodType> types, Guid paymentMethodTypeId) {
        var type = types.FirstOrDefault(x => x.Id == paymentMethodTypeId);
        return type?.Name ?? "Unknown";
    }
}