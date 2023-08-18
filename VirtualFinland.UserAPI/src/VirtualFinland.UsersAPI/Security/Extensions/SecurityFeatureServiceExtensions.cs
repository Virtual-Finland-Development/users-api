using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
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
    public static IServiceCollection RegisterSecurityFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        var features = new List<ISecurityFeature>();

        var securityConfigurations = configuration.GetSection("Security").Get<Dictionary<string, SecurityFeatureOptions>>();
        var enabledSecurityFeatureNames = securityConfigurations.Where(x => x.Value.IsEnabled).Select(x => x.Key).ToArray();
        if (!enabledSecurityFeatureNames.Any()) throw new ArgumentException("No security features enabled");

        // Map security feature name to correct class
        foreach (var securityFeatureName in enabledSecurityFeatureNames)
        {
            var securityFeatureType = Type.GetType($"VirtualFinland.UserAPI.Security.Features.{securityFeatureName}SecurityFeature");
            if (securityFeatureType is null) throw new ArgumentException($"Security feature {securityFeatureName} not found");

            var securityFeature = Activator.CreateInstance(securityFeatureType, securityConfigurations[securityFeatureName]) as ISecurityFeature;
            if (securityFeature is null) throw new ArgumentException($"Security feature {securityFeatureName} not found");

            features.Add(securityFeature);
        }

        services.AddSingleton<IApplicationSecurity>(new ApplicationSecurity(features));

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
            throw new NotAuthorizedException("Invalid token provided");

        var token = authorizationHeader["Bearer ".Length..].Trim();

        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new NotAuthorizedException("Invalid token provided");

        var issuer = jwtHandler.ReadJwtToken(token).Issuer;

        var feature = features.SingleOrDefault(securityFeature => securityFeature.Issuer == issuer);

        if (feature is null)
            throw new NotAuthorizedException("Invalid token provided");

        return feature.GetSecurityPolicySchemeName();
    }
}