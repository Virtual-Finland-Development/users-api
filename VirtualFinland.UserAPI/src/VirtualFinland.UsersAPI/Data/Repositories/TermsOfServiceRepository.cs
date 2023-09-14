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

        return await usersDbContext.TermsOfServices.OrderByDescending(t => t.Version).FirstOrDefaultAsync(CancellationToken.None) ?? throw new ArgumentException("No terms of service found");
    }

    public async Task<TermsOfService?> GetTermsOfServiceByVersion(string version)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.TermsOfServices.SingleOrDefaultAsync(t => t.Version == version, CancellationToken.None);
    }

    public async Task<List<PersonTermsOfServiceAgreement>> GetAllTermsOfServiceAgreementsByPersonId(Guid personId, string audience)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.PersonTermsOfServiceAgreements
            .Where(t => t.PersonId == personId && t.Audience == audience)
            .ToListAsync(CancellationToken.None);
    }

    public async Task<PersonTermsOfServiceAgreement?> GetTheLatestTermsOfServiceAgreementByPersonId(Guid personId, string audience)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.PersonTermsOfServiceAgreements
            .Where(t => t.PersonId == personId && t.Audience == audience)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(CancellationToken.None);
    }

    public async Task<PersonTermsOfServiceAgreement?> GetTermsOfServiceAgreementOfTheLatestTermsByPersonId(Guid personId, string audience)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Fetch the newest terms of service
        var termsOfService = await usersDbContext.TermsOfServices.OrderByDescending(t => t.Version).FirstOrDefaultAsync(CancellationToken.None) ?? throw new ArgumentException("No terms of service found");

        // Fetch person terms of service agreement
        return await usersDbContext.PersonTermsOfServiceAgreements.SingleOrDefaultAsync(t => t.PersonId == personId && t.TermsOfServiceId == termsOfService.Id, CancellationToken.None);
    }

    public async Task<PersonTermsOfServiceAgreement> AddNewTermsOfServiceAgreement(TermsOfService termsOfService, Guid personId, string audience)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var dbInsert = usersDbContext.PersonTermsOfServiceAgreements.Add(new PersonTermsOfServiceAgreement
        {
            PersonId = personId,
            TermsOfServiceId = termsOfService.Id,
            Version = termsOfService.Version,
            Audience = audience
        });

        await usersDbContext.SaveChangesAsync(CancellationToken.None);

        return dbInsert.Entity;
    }

    public async Task RemoveTermsOfServiceAgreement(PersonTermsOfServiceAgreement termsOfServiceAgreement)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        usersDbContext.PersonTermsOfServiceAgreements.Remove(termsOfServiceAgreement);

        await usersDbContext.SaveChangesAsync(CancellationToken.None);
    }
}