using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class TermsOfServiceRepository : ITermsOfServiceRepository
{
    private readonly UsersDbContext _usersDbContext;

    public TermsOfServiceRepository(UsersDbContext usersDbContext)
    {
        _usersDbContext = usersDbContext;
    }

    public async Task<TermsOfService> GetNewestTermsOfService()
    {
        return await _usersDbContext.TermsOfServices.OrderByDescending(t => t.Version).FirstOrDefaultAsync(CancellationToken.None) ?? throw new ArgumentNullException("No terms of service found");
    }

    public async Task<TermsOfService?> GetTermsOfServiceByVersion(string version)
    {
        return await _usersDbContext.TermsOfServices.SingleOrDefaultAsync(t => t.Version == version, CancellationToken.None);
    }

    public async Task<PersonTermsOfServiceAgreement?> GetNewestTermsOfServiceAgreementByPersonId(Guid personId)
    {
        // Fetch the newest terms of service
        var termsOfService = await GetNewestTermsOfService();

        // Fetch person terms of service agreement
        return await _usersDbContext.PersonTermsOfServiceAgreements.SingleOrDefaultAsync(t => t.PersonId == personId && t.TermsOfServiceId == termsOfService.Id, CancellationToken.None);
    }
}