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
        public const string NotAuthorized = "You are not authorized for this request!";
        public const string RequestBodyCannotBeEmpty = "Request body cannot by empty!";
        public const string AccessDenied = "Access denied for this request!";
        public const string AddressNotFound = "Address not found!";
        public const string AccountDeactivated = "This account has been deactivated!";
        public const string InvalidRestaurantId = "Invalid RestaurantId!";
        public const string RestaurantNotFound = "Restaurant Not Found!";
        public const string NullOrderRequestException = "Order Request cannot be null!";
        public const string EmptyOrderItemsException = "Order must contain at least one item!";
        public const string ItemQuantityException = "Item quantity must be greater than zero!";
        public const string DeliveryAddressNotFound = "Delivery Address not belongs to user!";
        public const string ItemNotFound = "One or More items not found!";
        public const string InsufficientQuanityError = "Insufficient quantity for menu item: {0}";
        public const string DuplicateMenuItemException = "One or more menu items have been added more than once!";
        public const string ItemsMustBelongToSameRestaurant = "All items in an order must belong to the same restaurant";
        public const string InsufficientBalance = "Insufficeint User balance!";
        public const string MenuItemNotFound = "MenuItem for this restaurant not found: {0}";
        public const string OrderNotFound = "Order not found!";
        public const string OrderCannotBeCancelled = "Order cannot be cancelled!";
        public const string RestaurantAlreadyExists = "Restaurant already exists!";
        public const string UserAlreadyExists = "User with this email already exists!";
        public const string OwnerDetailsRequired = "Owner details are required!";
        public const string ExistingUserError = "For an existing user, provide UserId only!";
        public const string RequiredOwnerDetails = "Name, email, phone number and password are required for a new owner!";
        public const string NotAuthenticated = "You are not authenticated for this route!";
        public const string NotAuthenticatedToUpdateStatus = "You are not authenticated to update order status of another restaurant!";
        public const string RoleMissingClaim = "User Role is missing!";
        public const string SameStatusError = "Order is already in this status!";
        public const string OrderStatusCannotChange = "Order Status cannot change now!";
        public const string RestaurantDoesNotBelongsToUser = "Requested Restaurant Don't Belongs to User!";
        public const string FrequentlyBoughtTogetherSizeError = "Frequently Bought Items size can be 2 or 3 only!";
        public const string DuplicateItemsNotAllowed = "Duplicate item IDs are not allowed!";
        public const string MenuItemsNotFound = "Menu item(s) not found: {0}";
        public const string FrequentlyBoughtItemsNotFound = "Frequently Bought Items for this size not found!";

    }
}
