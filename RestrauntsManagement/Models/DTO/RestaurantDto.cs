namespace DotNetRestaurantManagement.Models.DTO
{
    public class RestaurantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string Country { get; set; }
        public string Cuisine { get; set; }
    }
}
