using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Security.AccessRequirements;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Extensions;

public static class SecurityFeatureServiceExtensions
{
    /// <summary>
    ///     Configure security features using appsettings.json
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterSecurityFeatures(this IServiceCollection services, IConfiguration configuration, ConnectionMultiplexer redis)
    {
        var features = new List<ISecurityFeature>();

        var securityConfigurations = configuration.GetSection("Security:Authorization").Get<Dictionary<string, SecurityFeatureOptions>>();
        var enabledSecurityFeatureNames = securityConfigurations.Where(x => x.Value.IsEnabled).Select(x => x.Key).ToArray();
        if (!enabledSecurityFeatureNames.Any()) throw new ArgumentException("No security features enabled");

        var securityClientProviders = new SecurityClientProviders()
        {
            HttpClient = new HttpClient(
                new HttpRequestTimeoutHandler
                {
                    DefaultTimeout = TimeSpan.FromSeconds(6),
                    DefaultTimeoutMessage = "Security feature request timeout",
                    InnerHandler = new HttpClientHandler()
                }
            ),
            CacheRepositoryFactory = new CacheRepositoryFactory(redis.GetDatabase(), Constants.Security.CachePrefix),
        };

        // Dynamically map security feature name to correct class
        foreach (var securityFeatureName in enabledSecurityFeatureNames)
        {
            var securityFeatureType = Type.GetType($"VirtualFinland.UserAPI.Security.Features.{securityFeatureName}SecurityFeature") ?? throw new ArgumentException($"Security feature {securityFeatureName} not found");
            var securityFeature = Activator.CreateInstance(securityFeatureType, securityConfigurations[securityFeatureName], securityClientProviders) as ISecurityFeature ?? throw new ArgumentException($"Security feature {securityFeatureName} not found");
            features.Add(securityFeature);
        }

        // Register security setup
        services.AddSingleton(new SecuritySetup { Features = features, Options = configuration.GetSection("Security:Options").Get<SecurityOptions>() });

        // Register app security instance
        services.AddSingleton<IApplicationSecurity, ApplicationSecurity>();

        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
            options.DefaultChallengeScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
        });

        foreach (var securityFeature in features)
        {
            securityFeature.BuildAuthentication(authenticationBuilder);
        }

        services.AddAuthorization(options =>
        {
            foreach (var securityFeature in features) securityFeature.BuildAuthorization(options);

            // Add special policies
            options.AddPolicy(Constants.Security.RequestFromAccessFinland, policy =>
                policy.Requirements.Add(new RequestAccessRequirement(configuration.GetSection("Security:Access:AccessFinland").Get<RequestAccessConfig>())));
            options.AddPolicy(Constants.Security.RequestFromDataspace, policy =>
                policy.Requirements.Add(new RequestAccessRequirement(configuration.GetSection("Security:Access:Dataspace").Get<RequestAccessConfig>())));
        });

        authenticationBuilder.AddPolicyScheme(Constants.Security.ResolvePolicyFromTokenIssuer, Constants.Security.ResolvePolicyFromTokenIssuer,
            options =>
            {
                options.ForwardDefaultSelector =
                    context => GetSecurityPolicySchemeName(context.Request.Headers, features);
            });

        return services;
    }

    private static string GetSecurityPolicySchemeName(IHeaderDictionary headers, IEnumerable<ISecurityFeature> features)
    {
        var authorizationHeader = headers[HeaderNames.Authorization].FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            throw new NotAuthorizedException("No token provided");

        var token = authorizationHeader["Bearer ".Length..].Trim();

        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new NotAuthorizedException("Invalid token provided");

        var issuer = jwtHandler.ReadJwtToken(token).Issuer;

        var feature = features.SingleOrDefault(securityFeature => securityFeature.Issuer == issuer) ?? throw new NotAuthorizedException("Invalid token provider");
        return feature.GetSecurityPolicySchemeName();
    }
}