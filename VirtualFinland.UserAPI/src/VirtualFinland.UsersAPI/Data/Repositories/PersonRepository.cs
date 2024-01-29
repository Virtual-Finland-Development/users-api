using Microsoft.EntityFrameworkCore;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly IServiceProvider _services;


    public PersonRepository(IServiceProvider services)
    {
        _services = services;
    }

    public async Task DeletePerson(Guid personId, CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var person = await usersDbContext.Persons
            .Include(p => p.Occupations)
            .Include(p => p.Educations)
            .Include(p => p.LanguageSkills)
            .Include(p => p.Skills)
            .Include(p => p.Certifications)
            .Include(p => p.Permits)
            .Include(p => p.WorkPreferences)
            .Include(p => p.TermsOfServiceAgreements)
            .SingleAsync(p => p.Id == personId, cancellationToken);
        var externalIdentity = await usersDbContext.ExternalIdentities.SingleOrDefaultAsync(id => id.UserId == personId);

        // Actually remove
        usersDbContext.Persons.Remove(person);

        if (externalIdentity != null)
        {
            usersDbContext.ExternalIdentities.Remove(externalIdentity);
        }

        await usersDbContext.SaveChangesAsync(cancellationToken);
    }
}