using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class RestaurantResponse
    {
        public long RestaurantId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cuisine { get; set; }
        public RestaurantAddressDto Address { get; set; }
    }
}
