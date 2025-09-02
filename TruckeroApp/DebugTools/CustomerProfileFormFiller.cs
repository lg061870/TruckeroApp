namespace TruckeroApp.DebugTools;

using Truckero.Core.DTOs.Onboarding;

public static class CustomerProfileFormFiller
{
    public static Dictionary<string, string> UserAlreadyExists => new()
    {
        ["FullName"] = "Guillermo Jimenez",
        ["Email"] = "lg061870@gmail.com",
        ["PhoneNumber"] = "3477019838",
        ["Address"] = "San Jose, Costa Rica",
        ["Password"] = "Password123!",
        ["ConfirmPassword"] = "Password123!",
        ["AcceptTerms"] = "true"
    };

    public static Dictionary<string, string> AlwaysNewUser => new()
    {
        ["FullName"] = "New Jimenez",
        ["Email"] = "lg0618pp@gmail.com",
        ["PhoneNumber"] = "3477019838",
        ["Address"] = "San Jose, Costa Rica",
        ["Password"] = "Password123!",
        ["ConfirmPassword"] = "Password123!",
        ["AcceptTerms"] = "true"
    };

    public static Dictionary<string, string> InvalidEmailFormat => new()
    {
        ["FullName"] = "Bad Email Guy",
        ["Email"] = "not-an-email",
        ["PhoneNumber"] = "1234567890",
        ["Address"] = "Nowhere",
        ["Password"] = "Password123!",
        ["ConfirmPassword"] = "Password123!",
        ["AcceptTerms"] = "true"
    };

    public static Dictionary<string, string> MismatchedPasswords => new()
    {
        ["FullName"] = "Mismatch Man",
        ["Email"] = "mismatch@test.com",
        ["PhoneNumber"] = "1112223333",
        ["Address"] = "Mismatch Town",
        ["Password"] = "Password123!",
        ["ConfirmPassword"] = "Different123!",
        ["AcceptTerms"] = "true"
    };

    public static Dictionary<string, string> MissingFullName => new()
    {
        ["FullName"] = "",  // Intentionally left blank
        ["Email"] = "noname@test.com",
        ["PhoneNumber"] = "5556667777",
        ["Address"] = "Nameless Ave",
        ["Password"] = "Password123!",
        ["ConfirmPassword"] = "Password123!",
        ["AcceptTerms"] = "true"
    };
}

