using System.Text;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public static class ServerConfigurationValidation
{
    public static void ValidateServer(IConfiguration configuration)
    {
        var validationExceptions = new List<string>();

        if (string.IsNullOrEmpty(configuration["Testbed:OpenIDConfigurationURL"]))
        {
            validationExceptions.Add("Testbed:OpenIDConfigurationURL is missing");
        }

        if (string.IsNullOrEmpty(configuration["Sinuna:OpenIDConfigurationURL"]))
        {
            validationExceptions.Add("Sinuna:OpenIDConfigurationURL is missing");
        }

        if (string.IsNullOrEmpty(configuration["AuthGW:JwksJsonURL"]))
        {
            validationExceptions.Add("AuthGW:JwksJsonURL is missing");
        }

        if (string.IsNullOrEmpty(configuration["ExternalSources:CodesetApiBaseUrl"]))
        {
            validationExceptions.Add("ExternalSources:CodesetApiBaseUrl is missing");
        }

        if (validationExceptions.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            validationExceptions.ForEach(e => sb.AppendLine(e));

            throw new Exception(sb.ToString());
        }
    }
}