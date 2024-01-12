using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class SqsQueue
{
    /// <summary>
    /// Creates a new SQS queue that's used by the users API lambda to invoke the admin function lambda
    /// </summary>
    public static Queue CreateSqsQueueForAdminCommands(StackSetup stackSetup)
    {
        var dlq = new Queue(stackSetup.CreateResourceName("admin-function-sqs-dlq"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            Tags = stackSetup.Tags,
        });

        var queue = new Queue(stackSetup.CreateResourceName("admin-function-sqs"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            Tags = stackSetup.Tags,
            RedrivePolicy = dlq.Arn.Apply(arn => $@"{{
                ""deadLetterTargetArn"": ""{arn}"",
                ""maxReceiveCount"": 1
            }}")
        });

        return queue;
    }
}