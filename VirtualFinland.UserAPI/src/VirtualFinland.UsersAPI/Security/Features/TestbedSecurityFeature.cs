using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Security.Features;

public class TestbedSecurityFeature : ISecurityFeature
{
    private readonly string _openIDConfigurationURL;
    private string? _issuer;
    private string? _jwksOptionsUrl;
    private const int _configUrlMaxRetryCount = 5;
    /// <summary>
    /// How long to wait in milliseconds before trying again to retrieve the openid configs
    /// </summary>
    private const int _configUrlRetryWaitTime = 3000;

    public string? JwksOptionsUrl
    {
        get { return _jwksOptionsUrl; }
    }

    public string? Issuer
    {
        get { return _issuer; }
    }

    public TestbedSecurityFeature(IConfiguration configuration)
    {
        _openIDConfigurationURL = configuration["Testbed:OpenIDConfigurationURL"];
    }

    public void BuildAuthentication(AuthenticationBuilder authentication)
    {
        LoadOpenIdConfigUrl();

        authentication.AddJwtBearer(GetSecurityPolicySchemeName(), c =>
        {
            JwksExtension.SetJwksOptions(c, new JwkOptions(JwksOptionsUrl));

            c.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateActor = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer
            };
        });
    }

    public void BuildAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(GetSecurityPolicySchemeName(), policy =>
        {
            policy.AuthenticationSchemes.Add(GetSecurityPolicySchemeName());
            policy.RequireAuthenticatedUser();
        });
    }

    public string GetSecurityPolicySchemeName()
    {
        return "TestBedBearerScheme";
    }

    public string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Subject; // the "sub" claim
    }

    private async void LoadOpenIdConfigUrl()
    {
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(_openIDConfigurationURL);

        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                _issuer = jsonData?["issuer"]?.ToString();
                _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(_issuer) && !string.IsNullOrEmpty(_jwksOptionsUrl))
                {
                    break;
                }
            }
            await Task.Delay(_configUrlRetryWaitTime);
        }

        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new Exception("Failed to retrieve TestBed OpenID configurations.");
        }
    }

    public string GetSecurityPolicyScheme()
    {
        throw new NotImplementedException();
    }
}