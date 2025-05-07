public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetByUserIdAsync(Guid userId);
    Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId);
    Task AddAsync(PaymentMethod method);
    Task DeleteAsync(Guid id);
}

