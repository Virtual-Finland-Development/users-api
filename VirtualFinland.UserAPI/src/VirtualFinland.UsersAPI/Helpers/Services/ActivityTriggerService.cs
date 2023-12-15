using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using VirtualFinland.UserAPI.Models.App;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class ActivityTriggerService
{
    private readonly SqsSettings? _sqsSettings;
    private readonly IAmazonSQS _sqsClient;

    public ActivityTriggerService(IConfiguration configuration, IAmazonSQS sqsClient)
    {
        _sqsSettings = configuration.GetSection("Database:Triggers:SQS").Get<SqsSettings>();
        _sqsClient = sqsClient;
    }

    public async Task UpdatePersonActivity(Person person)
    {
        if (_sqsSettings is not null && _sqsSettings.IsEnabled)
        {
            // Only publish the event if person last activity is more than 1 days old
            if (person.LastActive is not null && person.LastActive > DateTime.UtcNow.AddDays(-1)) return;

            // Publish an SQS message to the lambda
            await _sqsClient.SendMessageAsync(new SendMessageRequest()
            {
                QueueUrl = _sqsSettings.QueueUrl,
                MessageGroupId = $"PersonActivityUpdates:{person.Id}", // Used in sqs dedublication
                MessageBody = JsonSerializer.Serialize(new
                {
                    Action = "UpdatePersonActivity",
                    Data = new { PersonId = person.Id }
                }),
            });
        }
    }
}
