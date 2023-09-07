using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class TermsOfServiceRepository : ITermsOfServiceRepository
{
    private readonly IServiceProvider _services;


    public TermsOfServiceRepository(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<TermsOfService> GetNewestTermsOfService()
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.TermsOfServices.OrderByDescending(t => t.Version).FirstOrDefaultAsync(CancellationToken.None) ?? throw new ArgumentNullException("No terms of service found");
    }

    public async Task<TermsOfService?> GetTermsOfServiceByVersion(string version)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.TermsOfServices.SingleOrDefaultAsync(t => t.Version == version, CancellationToken.None);
    }

    public async Task<PersonTermsOfServiceAgreement?> GetNewestTermsOfServiceAgreementByPersonId(Guid personId)
    {
        // Fetch the newest terms of service
        var termsOfService = await GetNewestTermsOfService();

        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Fetch person terms of service agreement
        return await usersDbContext.PersonTermsOfServiceAgreements.SingleOrDefaultAsync(t => t.PersonId == personId && t.TermsOfServiceId == termsOfService.Id, CancellationToken.None);
    }
}