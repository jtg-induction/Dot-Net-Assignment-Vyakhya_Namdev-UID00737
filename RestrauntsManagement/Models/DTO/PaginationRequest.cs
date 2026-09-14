using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(1, ValidationConstants.MaxPageLimit)]
        public int PageSize { get; set; } = 10;
    }
}
