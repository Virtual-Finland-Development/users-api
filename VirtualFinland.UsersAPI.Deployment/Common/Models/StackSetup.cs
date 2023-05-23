using Pulumi;

namespace VirtualFinland.UsersAPI.Deployment.Common.Models;

public record StackSetup
{
    public InputMap<string> Tags = default!;
    public bool IsProductionEnvironment;
    public string Environment = default!;
    public string ProjectName = default!;
    public VpcSetup VpcSetup = default!;
}