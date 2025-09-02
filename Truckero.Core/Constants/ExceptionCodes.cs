namespace Truckero.Core.Constants;

/// <summary>
/// Standardized error/exception codes for all service, API, and client layers.
/// Always use these constants for exception handling, API error responses, and client error checks.
/// </summary>
public static class ExceptionCodes {
    // --- Authentication & Authorization ---
    public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    public const string AccessTokenNotFound = "ACCESS_TOKEN_NOT_FOUND";
    public const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";
    public const string NoRefreshToken = "NO_REFRESH_TOKEN";
    public const string RefreshFailed = "REFRESH_FAILED";
    public const string RefreshError = "REFRESH_ERROR";
    public const string AccountNotAvailable = "ACCOUNT_NOT_AVAILABLE";
    public const string LoginExpired = "LOGIN_EXPIRED";
    public const string AccountDeleted = "ACCOUNT_DELETED";
    public const string UnauthorizedAccess = "UNAUTHORIZED_ACCESS";
    public const string AccessTokenIsBlank = "ACCESS_TOKEN_IS_BLANK";
    public const string InvalidHttpRequestSecurityHeader = "INVALID_HTTPREQUEST_SECURITY_HEADER";
    public const string NetworkError = "NETWORK_ERROR";
    public const string LoginFailure = "LOGIN_FAILURE";

    // --- User/Driver/Account ---
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string DriverProfileNotFound = "DRIVER_PROFILE_NOT_FOUND"; // Use this if you want to distinguish from UserNotFound

    // --- Validation & General Errors ---
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string IdMismatch = "ID_MISMATCH";
    public const string UnhandledException = "UNHANDLED_EXCEPTION";
    public const string Conflict = "CONFLICT";
    public const string NotImplemented = "NOT_IMPLEMENTED";

    // --- Truck & Vehicle Domain ---
    public const string TruckTypeNotFound = "TRUCK_TYPE_NOT_FOUND";
    public const string TruckModelNotFound = "TRUCK_MODEL_NOT_FOUND";
    public const string TruckAlreadyExists = "TRUCK_ALREADY_EXISTS";
    public const string TruckNotFound = "TRUCK_NOT_FOUND";
    public const string DuplicateLicensePlate = "DUPLICATE_LICENSE_PLATE";
    public const string MissingTruckCategory = "MISSING_TRUCK_CATEGORY";
    public const string MissingBedType = "MISSING_BED_TYPE";
    public const string MissingUseTag = "MISSING_USE_TAG";

    // --- Referential Integrity / Database ---
    public const string ReferentialIntegrityViolation = "REFERENTIAL_INTEGRITY_VIOLATION";
    public const string ForeignKeyNotFound = "FOREIGN_KEY_NOT_FOUND";
    public const string DbUpdateError = "DB_UPDATE_ERROR";
    public const string DbAddError = "DB_ADD_ERROR";
    public const string DbDeleteError = "DB_DELETE_ERROR";
    public const string DbSetDefaultError = "DB_SET_DEFAULT_ERROR";

    // --- Miscellaneous / Legacy ---
    public const string Unknown = "UNKNOWN";

    public static class FreightBidErrorCodes {
        public const string NotFound = "FREIGHTBID_NOT_FOUND";
        public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";
        public const string PreferredTruckTypeNotFound = "TRUCK_TYPE_NOT_FOUND";
        public const string AssignedTruckNotFound = "TRUCK_NOT_FOUND";
        public const string AssignedDriverNotFound = "DRIVER_NOT_FOUND";
        public const string PaymentMethodNotFound = "PAYMENT_METHOD_NOT_FOUND";
        public const string UseTagNotFound = "USETAG_NOT_FOUND";
        public const string FreightBidUseTagNotFound = "FREIGHTBIDUSETAG_NOT_FOUND";
        public const string SaveFailed = "FREIGHTBID_SAVE_FAILED";
        public const string ValidationError = "FREIGHTBID_VALIDATION_ERROR";
        public const string Conflict = "FREIGHTBID_CONFLICT";
        public const string Unknown = "FREIGHTBID_UNKNOWN";

        public static string DriverBidNotFound { get; set; }
    }


    public static class DriverBidErrorCodes {
        public const string NotFound = "DRIVER_BID_NOT_FOUND";
        public const string ForeignKeyNotFound = "DRIVER_BID_FOREIGN_KEY_NOT_FOUND";
        public const string DriverNotFound = "DRIVER_NOT_FOUND";
        public const string TruckNotFound = "TRUCK_NOT_FOUND";
        public const string FreightBidNotFound = "FREIGHT_BID_NOT_FOUND";
        public const string SaveFailed = "DRIVER_BID_SAVE_FAILED";
        public const string Unknown = "DRIVER_BID_UNKNOWN";
    }
    public static class PayoutAccountErrorCodes {
        public const string NotFound = "PAYOUT_ACCOUNT_NOT_FOUND";
        public const string CannotDeleteDefault = "PAYOUT_ACCOUNT_CANNOT_DELETE_DEFAULT";
        public const string InvalidPaymentMethodType = "PAYOUT_ACCOUNT_INVALID_PAYMENT_METHOD_TYPE";
        public const string UserNotFound = "PAYOUT_ACCOUNT_USER_NOT_FOUND";
        public const string DriverProfileNotFound = "PAYOUT_ACCOUNT_DRIVER_PROFILE_NOT_FOUND";
        public const string Unknown = "PAYOUT_ACCOUNT_UNKNOWN";
    }
    public static class PaymentAccountErrorCodes {
        public const string NotFound = "PAYMENT_ACCOUNT_NOT_FOUND";
        public const string InvalidType = "PAYMENT_ACCOUNT_INVALID_TYPE";
        public const string UserNotFound = "PAYMENT_ACCOUNT_USER_NOT_FOUND";
        public const string Unknown = "PAYMENT_ACCOUNT_UNKNOWN";
        // Add any others as needed!
    }
}
