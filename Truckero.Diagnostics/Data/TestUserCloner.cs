using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class TestUserCloner
{
    public static User CloneWithNewId(User source)
    {
        // Generate a unique email to avoid UNIQUE constraint violations
        var emailParts = source.Email.Split('@');
        var uniqueEmail = $"{emailParts[0]}_{Guid.NewGuid().ToString().Substring(0, 8)}@{emailParts[1]}";
        return new User
        {
            Id = Guid.NewGuid(),
            Email = uniqueEmail,
            FullName = source.FullName,
            PasswordHash = source.PasswordHash,
            PhoneNumber = source.PhoneNumber,
            Address = source.Address,
            RoleId = source.RoleId,
            EmailVerified = source.EmailVerified,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            Role = source.Role
        };
    }
}
