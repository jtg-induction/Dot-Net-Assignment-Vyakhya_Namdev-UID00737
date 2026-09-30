using System.Collections.Generic;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class MenuPaginationResult
    {
        public IEnumerable<MenuItemDto> MenuItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
