namespace VirtualFinland.UserAPI.Helpers;

public static class Constants
{
    public static class Web
    {
        public static string ServerUserAgent => "UsersAPI/1.0.0";
        public static string ClaimUserId => "userId";
    }

    public static class Security
    {
        public static string ResolvePolicyFromTokenIssuer => "ResolvePolicyFromTokenIssuer";
        public static string RequestFromAccessFinland => "RequestFromAccessFinland";
        public static string RequestFromDataspace => "RequestFromDataspace";
        public static string Anonymous => "Anonymous";
    }

    public static class Cache
    {
        public static string SecurityPrefix => "security";
        public static string OpenIdConfigPrefix => "openid-config";
        public static string CodesetsPrefix => "codesets";
    }

    public static class Headers
    {
        public static string XAuthorizationContext => "x-authorization-context";
        public static string XAuthorizationProvider => "x-authorization-provider";
        public static string XConsentToken => "x-consent-token";
        public static string XconsentDataSource => "x-consent-data-source";
        public static string XconsentUserId => "x-consent-user-id";
        public static string XRequestTraceId => "x-request-trace-id";
    }
}
