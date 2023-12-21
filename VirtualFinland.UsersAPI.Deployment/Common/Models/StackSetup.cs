using System.Threading.Tasks;
using Pulumi;

namespace VirtualFinland.UsersAPI.Deployment.Common.Models;

public class StackSetup
{
    public InputMap<string> Tags = default!;
    public bool IsProductionlikeEnvironment => Pulumi.Deployment.Instance.StackName == Environments.MvpStaging || Pulumi.Deployment.Instance.StackName == Environments.MvpProduction;
    public string Environment = default!;
    public string ProjectName = default!;
    public string Organization = default!;
    public string Region = default!;
    public string CreateResourceName(string name) => $"{ProjectName}-{name}-{Environment}".ToLower();
    public string GetInfrastructureStackName() => $"{Organization}/infrastructure/{Environment}";
    public string GetAlertingStackName() => $"{Organization}/cloudwatch-logs-alerts/{Environment}";

    public async Task<bool> IsInitialDeployment()
    {
        var currentStackReference = new StackReference($"{Organization}/{ProjectName}/{Environment}");
        // Check if output exists
        var output = await currentStackReference.GetValueAsync("ApplicationUrl");
        return output == null;
    }
}