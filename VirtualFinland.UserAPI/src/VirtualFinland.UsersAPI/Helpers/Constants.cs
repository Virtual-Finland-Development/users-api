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
        public static string TestBedBearerScheme => "DefaultTestBedBearerScheme";
        public static string SuomiFiBearerScheme => "SuomiFiBearerScheme";
        public static string SinunaScheme => "SinunaScheme";
        public static string AllPoliciesPolicy => "AllPolicies";
    }

    public static class Headers
    {
        public static string XAuthorizationContext => "x-authorization-context";
        public static string XAuthorizationProvider => "x-authorization-provider";
        public static string XConsentToken => "x-consent-token";
        public static string XconsentDataSource => "x-consent-data-source";
        public static string XconsentUserId => "x-consent-user-id";
    }
}
