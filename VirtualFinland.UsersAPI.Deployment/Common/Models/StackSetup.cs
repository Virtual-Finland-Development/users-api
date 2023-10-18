using Pulumi;

namespace VirtualFinland.UsersAPI.Deployment.Common.Models;

public class StackSetup
{
    public InputMap<string> Tags = default!;
    public bool IsProductionEnvironment;
    public string Environment = default!;
    public string ProjectName = default!;
    public string Organization = default!;
    public string Region = default!;
    public string CreateResourceName(string name) => $"{ProjectName}-{name}-{Environment}".ToLower();
    public string GetInfrastructureStackName() => $"{Organization}/infrastructure/{Environment}";
    public string GetAlertingStackName() => $"{Organization}/cloudwatch-logs-alerts/{Environment}";
}