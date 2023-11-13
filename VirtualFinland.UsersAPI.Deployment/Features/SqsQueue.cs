
using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class SqsQueue
{
    /// <summary>
    /// Creates a new SQS queue that's used by the users API lambda to invoke the admin function lambdas analytics updater command
    /// </summary>
    public static Queue CreateSqsQueueForAnalyticsCommand(StackSetup stackSetup)
    {
        return new Queue(stackSetup.CreateResourceName("analytics-update"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            Tags = stackSetup.Tags,
        });
    }
}