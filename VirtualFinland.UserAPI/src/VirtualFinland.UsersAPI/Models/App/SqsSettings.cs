namespace VirtualFinland.UserAPI.Models.App;

public record SqsSettings
{
    public bool IsEnabled { get; set; }
    public string QueueUrl { get; set; } = null!;
}