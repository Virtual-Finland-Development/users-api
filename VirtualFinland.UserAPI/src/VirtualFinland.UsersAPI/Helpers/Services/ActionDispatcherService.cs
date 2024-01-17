using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using VirtualFinland.UserAPI.Models.App;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class ActionDispatcherService
{
    private readonly SqsSettings? _sqsSettings;
    private readonly IAmazonSQS _sqsClient;

    public ActionDispatcherService(IConfiguration configuration, IAmazonSQS sqsClient)
    {
        _sqsSettings = configuration.GetSection("Dispatches:SQS").Get<SqsSettings>();
        _sqsClient = sqsClient;
    }

    public async Task UpdatePersonActivity(Person person)
    {
        if (_sqsSettings is null || !_sqsSettings.IsEnabled || string.IsNullOrEmpty(_sqsSettings.QueueUrls.Fast))
        {
            return;
        }

        // Only publish the event if person last activity is more than 1 days old
        // This is to avoid spamming the queue. With this the last activity set to update max daily
        if (person.LastActive is not null && person.LastActive > DateTime.UtcNow.AddDays(-1)) return;

        // Publish an SQS message to the lambda
        await _sqsClient.SendMessageAsync(new SendMessageRequest()
        {
            QueueUrl = _sqsSettings.QueueUrls.Fast,
            MessageGroupId = $"PersonActivityUpdates:{person.Id}", // Used in sqs dedublication
            MessageBody = JsonSerializer.Serialize(new
            {
                Action = "UpdatePerson",
                Data = JsonSerializer.Serialize(new
                {
                    PersonId = person.Id,
                    Type = "UpdateLastActiveDate"
                })
            }),
        });
    }

    public async Task UpdatePersonToBeDeletedFlag(Person person)
    {
        if (_sqsSettings is null || !_sqsSettings.IsEnabled || string.IsNullOrEmpty(_sqsSettings.QueueUrls.Slow))
        {
            return;
        }

        // Publish an SQS message to the lambda
        await _sqsClient.SendMessageAsync(new SendMessageRequest()
        {
            QueueUrl = _sqsSettings.QueueUrls.Slow,
            MessageBody = JsonSerializer.Serialize(new
            {
                Action = "UpdatePerson",
                Data = JsonSerializer.Serialize(new
                {
                    PersonId = person.Id,
                    Type = "UpdateToBeDeletedFlag"
                })
            }),
        });
    }

    public async Task DeleteAbandonedPerson(Person person)
    {
        if (_sqsSettings is null || !_sqsSettings.IsEnabled || string.IsNullOrEmpty(_sqsSettings.QueueUrls.Slow))
        {
            return;
        }

        // Publish an SQS message to the lambda
        await _sqsClient.SendMessageAsync(new SendMessageRequest()
        {
            QueueUrl = _sqsSettings.QueueUrls.Slow,
            MessageBody = JsonSerializer.Serialize(new
            {
                Action = "UpdatePerson",
                Data = JsonSerializer.Serialize(new
                {
                    PersonId = person.Id,
                    Type = "DeleteAbandonedPerson"
                })
            }),
        });
    }
}
