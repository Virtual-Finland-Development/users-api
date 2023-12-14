using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data;

public class DatabaseActivityInterceptor : DbCommandInterceptor
{
    private readonly DatabaseEventTriggersService _eventTriggers;

    public DatabaseActivityInterceptor(DatabaseEventTriggersService eventTriggers)
    {
        _eventTriggers = eventTriggers;
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await result.ReadAsync(cancellationToken);

            // If result if of type Person, update the person's activity
            if (result.HasRows)
            {
                // Check if the result is of type Person
                if (result.GetDataTypeName(0) != nameof(Person)) return result;

                // Update the person's activity
                await _eventTriggers.UpdatePersonActivity((Person)result[0]);
            }
        }
        finally
        {
            await result.DisposeAsync();
        }

        return result;
    }
}