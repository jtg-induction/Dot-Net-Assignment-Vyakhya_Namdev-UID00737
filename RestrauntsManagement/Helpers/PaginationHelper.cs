using DotNetRestaurantManagement.Models.DTO;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Helpers
{
    public class PaginationHelper
    {
        public static async Task<PaginationResult<T>> CreateAsync<T>(IQueryable<T> query, PaginationRequest request)
        {
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginationResult<T>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = request.Page > 1,
                HasNextPage = request.Page < totalPages
            };
        }
    }
}
