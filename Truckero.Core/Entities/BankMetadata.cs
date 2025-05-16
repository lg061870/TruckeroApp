public class BankMetadata
{
    public string BankName { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public string AccountNumberMasked { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // e.g., Checking
}
