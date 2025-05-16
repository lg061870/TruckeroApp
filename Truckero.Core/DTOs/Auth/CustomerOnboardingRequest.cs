namespace Truckero.Core.DTOs.Auth
{
    public class CustomerOnboardingRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public string? Email { get; set; }             
        public string? Password { get; set; }          
        public bool HasPaymentMethod { get; set; }     
    }

}