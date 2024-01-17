namespace VirtualFinland.UserAPI.Models.App;

public record SqsSettings
{
    public bool IsEnabled { get; set; }
    public SqsQueueUrls QueueUrls { get; set; } = null!;
}

public record SqsQueueUrls
{
    public string Fast { get; init; } = null!;
    public string Slow { get; init; } = null!;
}