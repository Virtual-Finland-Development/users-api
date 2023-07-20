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
        var enabledSecurityFeatures = configuration.GetSection("Security:EnabledSecurityFeatures").Get<string[]>();
        foreach (var securityFeature in enabledSecurityFeatures)
        {
            switch (securityFeature)
            {
                case "Testbed":
                    features.Add(new TestbedSecurityFeature(configuration));
                    break;
                case "Sinuna":
                    features.Add(new SinunaSecurityFeature(configuration));
                    break;
                case "SuomiFi":
                    features.Add(new SuomiFiSecurityFeature(configuration));
                    break;
                default:
                    throw new NotImplementedException($"Security feature {securityFeature} is not implemented");
            }
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