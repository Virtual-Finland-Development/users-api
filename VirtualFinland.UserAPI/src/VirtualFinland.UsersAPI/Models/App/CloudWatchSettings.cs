namespace VirtualFinland.UserAPI.Models.App;

public record CloudWatchSettings
{
    public bool IsEnabled { get; set; }
    public string Namespace { get; set; } = null!;
}