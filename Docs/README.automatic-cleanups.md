# Automatic cleanups

## Abandoned user accounts

As per service terms of use, user accounts that have not been used for 36 months are considered abandoned and will be removed from the system. The cleanup is performed by the [../VirtualFinland.UsersAPI.AdminFunction](../VirtualFinland.UsersAPI.AdminFunction) lambda function with an `RunCleanups` action that is scheduled to run once a day.

The cleanup is performed in two phases:

1. The user accounts that have not been used for 36 months are marked as abandoned
2. The user accounts that have been marked as abandoned are removed from the system after 30 days of continued user inactivity

From both phases the user is notified by email about the upcoming cleanup if the email notifications are enabled (see: [./README.notifications.md](./README.notifications.md)).

### Configuration

The cleanup is configured in the appsettings.json file with the following configuration:

```json
"Cleanups": {
    "AbandonedAccounts": {
        "Enabled": true,
        "FlagAsAbandonedInDays": 1095,
        "DeleteFlaggedAfterDays": 30,
        "MaxPersonsToFlagPerDay": 1000
    }
}
```

- `Enabled` - if the cleanup is enabled or not
- `FlagAsAbandonedInDays` - the number of days after which the user account is flagged as abandoned
- `DeleteFlaggedAfterDays` - the number of days after which the flagged user account is deleted from the system
- `MaxPersonsToFlagPerDay` - the maximum number of user accounts that are flagged as abandoned per day