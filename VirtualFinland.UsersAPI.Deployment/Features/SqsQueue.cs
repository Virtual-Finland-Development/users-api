using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class SqsQueue
{
    /// <summary>
    /// Creates a new SQS queue that's used by the users API lambda to invoke the admin function lambda
    /// </summary>
    public static Queues CreateSqsQueueForAdminCommands(StackSetup stackSetup)
    {
        // Queue from "fast track", first-in-first-out events with deduplication
        var fastDlq = new Queue(stackSetup.CreateResourceName("admin-function-fast-sqs-dlq"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            MessageRetentionSeconds = 3600, // 1 hour
            Tags = stackSetup.Tags,
        });
        var fastQueue = new Queue(stackSetup.CreateResourceName("admin-function-fast-sqs"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            MessageRetentionSeconds = 3600,
            Tags = stackSetup.Tags,
            RedrivePolicy = fastDlq.Arn.Apply(arn => $@"{{
                ""deadLetterTargetArn"": ""{arn}"",
                ""maxReceiveCount"": 1
            }}")
        });

        // Queue for "slow track", standard queue events, failed events are retried until retention period is reached
        var slowQueue = new Queue(stackSetup.CreateResourceName("admin-function-slow-sqs"), new QueueArgs
        {
            FifoQueue = false,
            VisibilityTimeoutSeconds = 120,
            MessageRetentionSeconds = 86400,
            DelaySeconds = 5,
            Tags = stackSetup.Tags,
        });

        return new Queues
        {
            Fast = fastQueue,
            Slow = slowQueue
        };
    }

    public record Queues
    {
        public Queue Fast { get; init; } = null!;
        public Queue Slow { get; init; } = null!;
    }
}