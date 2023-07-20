using System.Text;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public static class ServerConfigurationValidation
{
    public static void ValidateServer(IConfiguration configuration)
    {
        var validationExceptions = new List<string>();

        if (configuration.GetSection("Security:EnabledSecurityFeatures").Get<string[]>()?.Length == 0)
        {
            validationExceptions.Add("Security:EnabledSecurityFeatures is missing");
        }

        if (string.IsNullOrEmpty(configuration["CodesetApiBaseUrl"]))
        {
            validationExceptions.Add("CodesetApiBaseUrl is missing");
        }

        if (validationExceptions.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            validationExceptions.ForEach(e => sb.AppendLine(e));

            throw new Exception(sb.ToString());
        }
    }
}