namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class CleanupConfig
{
    public readonly AbandonedAccountsConfig AbandonedAccounts;

    public CleanupConfig(IConfiguration configuration)
    {
        AbandonedAccounts = configuration.GetSection("Cleanups:AbandonedAccounts").Get<AbandonedAccountsConfig>();
    }

    public record AbandonedAccountsConfig
    {
        public bool IsEnabled { get; init; }
        public int FlagAsAbandonedInDays { get; init; }
        public int DeleteFlaggedAfterDays { get; init; }
        public int MaxPersonsToFlagPerDay { get; init; }
    }
}