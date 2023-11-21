namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class CodesetConfig
{
    private readonly CodesetsServiceConfig _codesetsServiceConfig;

    public CodesetConfig(IConfiguration configuration)
    {
        _codesetsServiceConfig = configuration.GetSection("Services:Codesets").Get<CodesetsServiceConfig>();
        if (string.IsNullOrEmpty(_codesetsServiceConfig.ApiEndpoint))
        {
            throw new ArgumentException("Services:Codesets.ApiEndpoint not defined");
        }
    }

    public string GetResourceEndpoint(CodesetsResource resource)
    {
        return $"{_codesetsServiceConfig.ApiEndpoint}/{resource}";
    }

    private record CodesetsServiceConfig
    {
        public string ApiEndpoint { get; init; } = string.Empty;
        public int ServiceRequestTimeoutInMilliseconds { get; init; } = 9000;
    }
}