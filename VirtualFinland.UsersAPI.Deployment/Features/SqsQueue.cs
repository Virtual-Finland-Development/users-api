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
        var fastDlq = new Queue(stackSetup.CreateResourceName("admin-function-fast-sqs-dlq"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            Tags = stackSetup.Tags,
        });
        var fastQueue = new Queue(stackSetup.CreateResourceName("admin-function-fast-sqs"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            Tags = stackSetup.Tags,
            RedrivePolicy = fastDlq.Arn.Apply(arn => $@"{{
                ""deadLetterTargetArn"": ""{arn}"",
                ""maxReceiveCount"": 1
            }}")
        });

        var slowDlq = new Queue(stackSetup.CreateResourceName("admin-function-slow-sqs-dlq"), new QueueArgs
        {
            FifoQueue = false,
            VisibilityTimeoutSeconds = 120,
            Tags = stackSetup.Tags,
            MaxMessageSize = 262144,
            MessageRetentionSeconds = 345600,
            DelaySeconds = 5,
        });
        var slowQueue = new Queue(stackSetup.CreateResourceName("admin-function-slow-sqs"), new QueueArgs
        {
            FifoQueue = false,
            VisibilityTimeoutSeconds = 120,
            Tags = stackSetup.Tags,
            RedrivePolicy = slowDlq.Arn.Apply(arn => $@"{{
                ""deadLetterTargetArn"": ""{arn}"",
                ""maxReceiveCount"": 1
            }}")
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