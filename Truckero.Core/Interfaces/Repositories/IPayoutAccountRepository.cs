public interface IPayoutAccountRepository
{
    Task<List<PayoutAccount>> GetByUserIdAsync(Guid userId);
    Task<PayoutAccount?> GetDefaultByUserIdAsync(Guid userId);
    Task AddAsync(PayoutAccount account);
    Task DeleteAsync(Guid id);
}
