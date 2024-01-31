using System.Text.Json;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Flag for deletion the persons that have not been active in years
/// A month after the delete-flagging, delete them
/// </summary>
public class SendEmailAction : IAdminAppAction
{
    private readonly AmazonSimpleEmailServiceClient _client;
    private readonly ILogger<SendEmailAction> _logger;

    public SendEmailAction(AmazonSimpleEmailServiceClient client, ILogger<SendEmailAction> logger)
    {
        _logger = logger;
        _client = client;
    }

    public async Task Execute(string? input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        var sendRequest = JsonSerializer.Deserialize<SendEmailRequest>(input) ??
            throw new ArgumentException("Invalid input", nameof(input));

        _logger.LogInformation("Sending email to {toAddresses}", sendRequest.Destination.ToAddresses);

        await _client.SendEmailAsync(sendRequest);
    }
}