
using System.Collections.Generic;
using System.Text.Json;
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
        var queue = new Queue(stackSetup.CreateResourceName("analytics-update"), new QueueArgs
        {
            FifoQueue = true,
            ContentBasedDeduplication = true,
            DeduplicationScope = "messageGroup",
            VisibilityTimeoutSeconds = 30,
            FifoThroughputLimit = "perMessageGroupId",
            Tags = stackSetup.Tags,
        });

        return queue;
    }
}