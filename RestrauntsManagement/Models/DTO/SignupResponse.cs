namespace DotNetRestaurantManagement.Models.DTO
{
    public class SignupResponse
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public string Message { get; set; }
    }
}