namespace DotNetRestaurantManagement.Constants
{
    public class ErrorMessages
    {
        public const string InvalidPhoneNumber = "Please Enter Valid Phone Number!";
        public const string InvalidPinCode = "Pin code must contain 6 digits!";
        public const string InvalidPassword = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character";
        public const string PasswordEmptyError = "Password cannot be empty!*";
        public const string UnexpectedError = "An unexpected error occurred!";
        public const string DuplicateEmailException = "Email already exists!";
        public const string DuplicatePhoneNumberException = "Phone number already exists!";
        public const string UserIdMissingClaim = "User ID claim is missing or invalid!";
        public const string RefreshTokenIdMissingClaim = "Refresh token ID claim is missing or invalid!";
        public const string RefreshTokenEmptyValidation = "Refresh token cannot be empty!";
        public const string InvalidCredentials = "Invalid Credentials Provided!";
        public const string UserNotFound = "User Not Found!";
        public const string PasswordUpdateValidation = "New password must be different from the current password!";
        public const string PhoneNumberAlreadyExists = "Phone Number already exists!";
        public const string PasswordMatchesError = "New Password should not be same as Current Password!";
        public const string InvalidRefreshToken = "Invalid Refresh Token Provided!";
        public const string InvalidRestaurantId = "RestaurantId must be greater than 0!";
        public const string RestaurantNotFound = "Restaurant Not Found!";
        public const string RestaurantNotAvailable = "Restaurant Not Available!";
        public const string NullOrderRequestException = "Order Request cannot be null!";
        public const string EmptyOrderItemsException = "Order must contain at least one item!";
        public const string ItemQuantityException = "Item quantity must be greater than zero!";
        public const string AddressNotFound = "Delivery Address not belongs to user!";
        public const string ItemNotFound = "One or More items not found!";
        public const string InsufficientQuanityError = "Insufficient quantity for menu item";
        public const string DuplicateMenuItemException = "Duplicate menu items are not allowed!";
        public const string ItemsMustBelongToSameRestaurant = "All items in an order must belong to the same restaurant";
        public const string InsufficientBalance = "Insufficeint User balance!";
    }
}
