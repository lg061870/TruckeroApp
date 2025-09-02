using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

public class CustomerProfileService : ICustomerProfileService {
    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CustomerProfileService> _logger;

    public CustomerProfileService(
        ICustomerProfileRepository customerProfileRepository,
        IUserRepository userRepository,
        ILogger<CustomerProfileService> logger) {
        _customerProfileRepository = customerProfileRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CustomerProfileResponse> GetByUserByCustomerProfileIdAsync(Guid userId) {
        try {
            var profile = await _customerProfileRepository.GetCustomerProfileByUserIdAsync(userId);

            if (profile == null) {
                return new CustomerProfileResponse {
                    Success = false,
                    Message = "Customer profile not found.",
                    ErrorCode = "PROFILE_NOT_FOUND"
                };
            }

            // Get user properties from related User entity
            var user = profile.User;
            var dto = new CustomerProfileRequest {
                Id = profile.Id,
                UserId = profile.UserId,
                CreatedAt = profile.CreatedAt // Or use a different date property if you add one
            };

            return new CustomerProfileResponse {
                Success = true,
                Message = "Customer profile found.",
                CustomerProfiles = new List<CustomerProfileRequest> { dto }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error in GetByUserIdAsync for userId={UserId}", userId);
            return new CustomerProfileResponse {
                Success = false,
                Message = "An unexpected error occurred while retrieving the profile.",
                ErrorCode = "UNKNOWN_ERROR"
            };
        }
    }

    public async Task<CustomerProfileResponse> CreateCustomerProfileAsync(CustomerProfileRequest request, Guid userId) {
        try {
            // 1. Fetch the User (must exist, since CustomerProfile is tied to User)
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) {
                return new CustomerProfileResponse {
                    Success = false,
                    Message = "User not found.",
                    ErrorCode = "USER_NOT_FOUND"
                };
            }

            await _userRepository.SaveUserChangesAsync();

            // 3. Create CustomerProfile (one per user)
            var profile = new CustomerProfile {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                // Any other CustomerProfile fields (not in your DTO currently)
            };

            await _customerProfileRepository.AddCustomerProfileAsync(profile);
            await _customerProfileRepository.SaveCustomerProfileChangesAsync();

            // 4. Prepare response (returning DTO shape, not entity)
            var customerProfileDto = new CustomerProfileRequest {
                Id = profile.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow // or profile.CreatedAt if you want
            };

            return new CustomerProfileResponse {
                Success = true,
                Message = "Customer profile created.",
                CustomerProfiles = new List<CustomerProfileRequest> { customerProfileDto }
            };
        } catch (DbUpdateException dbex) {
            _logger.LogError(dbex, "Database error in CreateAsync for userId={UserId}", userId);
            return new CustomerProfileResponse {
                Success = false,
                Message = "Database error occurred while creating profile.",
                ErrorCode = "DB_UPDATE_ERROR"
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in CreateAsync for userId={UserId}", userId);
            return new CustomerProfileResponse {
                Success = false,
                Message = "An unexpected error occurred.",
                ErrorCode = "UNKNOWN_ERROR"
            };
        }
    }
}
