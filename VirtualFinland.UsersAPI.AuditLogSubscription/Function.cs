using Amazon.Lambda.Core;
using Amazon.Lambda.CloudWatchLogsEvents;
using System.Text.Json;
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace VirtualFinland.AuditLogSubscription;

public class Function
{
    public static void FunctionHandler(CloudWatchLogsEvent logsEvent)
    {
        var parsed = JsonSerializer.Deserialize<CloudWatchEventData>(logsEvent.Awslogs.DecodeData(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Could not deserialize CloudWatch event data");

        foreach (var logEvent in parsed.LogEvents)
        {
            Console.WriteLine(logEvent.Message); // Log the auditlog event data
        }
    }

    private record CloudWatchEventData
    {
        public string MessageType { get; init; } = default!;
        public string Owner { get; init; } = default!;
        public string LogGroup { get; init; } = default!;
        public string LogStream { get; init; } = default!;
        public List<string> SubscriptionFilters { get; init; } = new();
        public List<CloudWatchLogEvent> LogEvents { get; init; } = new();
    }
    private record CloudWatchLogEvent
    {
        public string Id { get; init; } = default!;
        public long Timestamp { get; init; } = default!;
        public string Message { get; init; } = default!;
    }
}