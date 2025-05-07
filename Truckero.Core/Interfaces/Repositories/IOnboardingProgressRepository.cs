using Truckero.Core.Entities;

public interface IOnboardingProgressRepository
{
    Task<OnboardingProgress?> GetByUserIdAsync(Guid userId);
    Task AddOrUpdateAsync(OnboardingProgress progress);
    Task SaveChangesAsync();
}
