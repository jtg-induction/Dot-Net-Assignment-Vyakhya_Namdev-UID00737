using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}