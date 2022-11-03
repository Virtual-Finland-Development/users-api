using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NetDevPack.Security.JwtExtensions;

namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class JwksExtension
{
    public static void SetJwksOptions(this JwtBearerOptions options, JwkOptions jwkOptions)
    {
        HttpClient httpClient = new HttpClient(options.BackchannelHttpHandler ?? (HttpMessageHandler) new HttpClientHandler())
        {
            Timeout = options.BackchannelTimeout,
            MaxResponseContentBufferSize = 10485760
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.Web.ServerUserAgent);

        options.ConfigurationManager = (IConfigurationManager<OpenIdConnectConfiguration>) new ConfigurationManager<OpenIdConnectConfiguration>(jwkOptions.JwksUri, (IConfigurationRetriever<OpenIdConnectConfiguration>) new JwksRetriever(), (IDocumentRetriever) new HttpDocumentRetriever(httpClient)
        {
            RequireHttps = options.RequireHttpsMetadata
        });
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidIssuer = jwkOptions.Issuer;
        if (string.IsNullOrEmpty(jwkOptions.Audience))
            return;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = jwkOptions.Audience;
    }
}