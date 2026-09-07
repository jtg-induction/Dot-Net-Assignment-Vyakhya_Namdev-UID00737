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
    }
}
