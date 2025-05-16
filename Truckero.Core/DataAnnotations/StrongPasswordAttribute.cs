using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Truckero.Core.DataAnnotations;

public class StrongPasswordAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var password = value as string;
        if (string.IsNullOrWhiteSpace(password)) return false;

        // At least 8 chars, one uppercase, one lowercase, one digit, one special
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
    }
}

