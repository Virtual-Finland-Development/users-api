namespace VirtualFinland.UserAPI.Helpers;

public static class Constants
{
    public static class Web
    {
        public static string ServerUserAgent
        {
            get
            {
                return "UsersAPI/1.0.0";
            }
        }

        public static string ClaimUserId
        {
            get
            {
                return "userId";
            }
        }

        public static string AuthGwApplicationContext
        {
            get
            {
                return "user-api-productizer";
            }
        }
    }
    public static class Security
    {
        public static string TestBedBearerScheme
        {
            get
            {
                return "DefaultTestBedBearerScheme";
            }
        }

        public static string SuomiFiBearerScheme
        {
            get
            {
                return "SuomiFiBearerScheme";
            }
        }

        public static string SinunaScheme
        {
            get
            {
                return "SinunaScheme";
            }
        }

        public static string AllPoliciesPolicy
        {
            get
            {
                return "AllPolicies";
            }
        }
    }

    public static class Headers
    {
        public static string XAuthorizationContext
        {
            get
            {
                return "x-authorization-context";
            }
        }

        public static string XAuthorizationProvider
        {
            get
            {
                return "x-authorization-provider";
            }
        }

        public static string XConsentToken
        {
            get
            {
                return "x-consent-token";
            }
        }
    }
}