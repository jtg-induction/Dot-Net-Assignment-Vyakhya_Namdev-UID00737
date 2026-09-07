namespace DotNetRestaurantManagement.Constants
{
    public class RegexConstants
    {
        public const string PhoneNumberRegex = @"^[6-9]\d{9}$";
        public const string PinCodeRegex = @"^[1-9][0-9]{5}$";
        public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s])\S+$";
        public const string RefreshTokenRegex = @"^[A-Za-z0-9+/]*={0,2}$";
    }
}
