using DotNetRestaurantManagement.Models.DTO;
using System.Collections.Generic;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IGenerateReportService
    {
        byte[] Generate<T>(
                    IEnumerable<T> reportData,
                    string reportFileName,
                    bool? showRestaurantData = null,
                    int? itemCount = null);
    }
}
