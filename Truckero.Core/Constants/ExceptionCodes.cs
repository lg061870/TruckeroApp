namespace Truckero.Core.Constants;

public static class ExceptionCodes
{
    public const string EmailNotVerified = "email_not_verified";
    public const string AccessTokenNotFound = "accesstoken_not_found";
    public const string AccessTokenExpired = "accesstoken_expired";
    public const string NoRefreshToken = "no-refresh-token";
    public const string RefreshFailed = "refresh-failed";
    public const string RefreshError = "refresh-error";
    public const string AccountNotAvailable = "account-not-available";
    public const string LoginExpired = "login-expired";
    public const string AccountDeleted = "account-fully-deleted";
    public const string UnauthorizedAccess = "unauthorized-access";
    public const string AccessTokenIsBlank = "accesstoken-is-blank";
    public const string InvalidHttpRequestSecurityHeader = "invalid-httprequest-securityheader";
    public const string NetworkError = "network-error";
    public const string LoginFailure = "login-failure";
}
