namespace DotNetRestaurantManagement.Constants
{
    public static class ValidationConstants
    {
        public const int MaxNameLength = 150;
        public const int MaxTextLength = 255;
        public const int MaxLocationLength = 100;
        public const int HouseNumberMaxLength = 50;
        public const int MinimumPasswordLength = 8;
        public const int MaximumPasswordLength = 50;
        public const int PhoneNumberLength = 10;
        public const int PinCodeLength = 6;
        public const int MinimumQuantity = 1;
        public const int MinimumPreparationTime = 10;
        public const decimal MaximumOrderAmount = 1000000;
        public const decimal MaximumOrderItemPrice = 10000000;
        public const decimal DefaultUserBalance = 1000;
        public const int RefreshTokenLength = 88;
        public const int RefreshTokenExpiryDays = 7;
    }
}
