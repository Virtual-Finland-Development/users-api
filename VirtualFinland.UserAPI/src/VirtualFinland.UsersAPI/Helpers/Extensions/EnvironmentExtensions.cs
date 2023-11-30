namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class Environments
{
    public static readonly string Local = "local";
    public static readonly string Development = "dev";
    public static readonly string Staging = "staging";
    public static readonly string Production = "production";
    public static readonly string MvpDevelopment = "mvp-dev"; // Not in use but should be
    public static readonly string MvpStaging = "mvp-staging";
    public static readonly string MvpProduction = "mvp-production";
}

public static class EnvironmentExtensions
{
    public static bool IsLocal(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.Local);
    }

    //
    // Non-MVP app environments
    //
    public static bool IsDevelopment(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.Development);
    }

    public static bool IsStaging(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.Staging);
    }

    public static bool IsProduction(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.Production);
    }

    // Production-like environments are staging and production
    public static bool IsProductionlike(this IHostEnvironment hostEnvironment)
    {
        return IsProduction(hostEnvironment) || IsStaging(hostEnvironment);
    }

    //
    // MVP app environments
    //
    public static bool IsMvpDevelopment(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.MvpDevelopment);
    }

    public static bool IsMvpStaging(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.MvpStaging);
    }

    public static bool IsMvpProduction(this IHostEnvironment hostEnvironment)
    {
        return IsEnvironment(hostEnvironment, Environments.MvpProduction);
    }

    // MVP production-like environments are MVP staging and MVP production

    public static bool IsMvpProductionlike(this IHostEnvironment hostEnvironment)
    {
        return IsMvpProduction(hostEnvironment) || IsMvpStaging(hostEnvironment);
    }

    // 
    // Helper methods
    //
    public static bool IsMvpEnvironment(this IHostEnvironment hostEnvironment)
    {
        return IsMvpProduction(hostEnvironment) || IsMvpStaging(hostEnvironment) || IsMvpDevelopment(hostEnvironment);
    }

    public static bool IsEnvironment(IHostEnvironment hostEnvironment, string environmentName)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(environmentName);
    }
}