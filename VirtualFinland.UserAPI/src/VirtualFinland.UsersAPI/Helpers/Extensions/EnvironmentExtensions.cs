namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class Environments
{
    public static readonly string Local = "local";
    public static readonly string Development = "dev";
    public static readonly string Staging = "staging";
    public static readonly string Production = "production";
    public static readonly string MvpStaging = "mvp-staging";
    public static readonly string MvpProduction = "mvp-production";
}

public static class EnvironmentExtensions
{
    public static bool IsLocal(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.Local);
    }

    public static bool IsDevelopment(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.Development);
    }

    public static bool IsStaging(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentException(null, nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.MvpStaging) || hostEnvironment.IsEnvironment(Environments.Staging);
    }

    public static bool IsProduction(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentException(null, nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.MvpProduction) || hostEnvironment.IsEnvironment(Environments.Production);
    }
}