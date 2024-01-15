# Notifications

# Email Notifications

The email notifications are sent using the AWS Simple Email Service (SES). The SES service is configured in the [infrastructure](https://github.com/Virtual-Finland-Development/infrastructure) repository and. The email notications are configured in the appsettings.json file `Notifications` block with the following configuration:

```json
"Email": {
    "IsEnabled": true,
    "FromAddress": "no-reply@dev.accessfinland.dev",
    "SiteUrl": "https://dev.accessfinland.dev"
}
```

- `IsEnabled` - Enables or disables the email notifications
- `FromAddress` - The email address that is used as the sender of the email notifications (must be configured in the AWS SES)
- `SiteUrl` - The URL of the site that is used in the email notification templates

## Email Templates

The contents for the sent email notifications are defined in the [EmailTemplates.cs](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Helpers/EmailTemplates.cs) helper class.