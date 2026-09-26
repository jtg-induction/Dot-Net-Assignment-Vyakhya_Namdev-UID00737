namespace DotNetRestaurantManagement.Models.DTO
{
    public class Top10OrderedItemsReport
    {
        public long? RestaurantId { get; set; }
        public string RestaurantName { get; set; }

        public long MenuItemId { get; set; }
        public string MenuItemName { get; set; }

        public int NumberOfTimesOrdered { get; set; }
    }
}
