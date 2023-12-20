namespace VirtualFinland.UsersAPI.Deployment.Common;

public static class Environments
{
    public static string Dev => "dev";
    public static string Staging => "staging";
    public static string MvpStaging => "mvp-staging";
    public static string MvpProduction => "mvp-production";

    public static bool IsProductionEnvironment()
    {
        var stackName = Pulumi.Deployment.Instance.StackName;
        return stackName switch
        {
            // Cheers: https://stackoverflow.com/a/65642709
            var value when value == MvpProduction => true,
            var value when value == MvpStaging => true,
            _ => false,
        };
    }
}