public class PaymentMethodType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}
