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

        public static string ClaimNameId
        {
            get
            {
                return "nameID";
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
}