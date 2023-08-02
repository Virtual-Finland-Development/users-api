using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Data.Configuration;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Security;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext
{
    private readonly bool _isTesting;
    public readonly ICryptoUtility Cryptor;
    private readonly IAuditInterceptor _auditInterceptor;

    public UsersDbContext(DbContextOptions options, IDatabaseEncryptionSettings secrets, IAuditInterceptor auditInterceptor) : base(options)
    {
        Cryptor = new CryptoUtility(secrets);
        _auditInterceptor = auditInterceptor;
    }

    public UsersDbContext(DbContextOptions options, IDatabaseEncryptionSettings secrets, IAuditInterceptor auditInterceptor, bool isTesting) : base(options)
    {
        Cryptor = new CryptoUtility(secrets);
        _auditInterceptor = auditInterceptor;
        _isTesting = isTesting;
    }

    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
    //public DbSet<ExternalIdentityAccessKey> ExternalIdentityAccessKeys => Set<ExternalIdentityAccessKey>();
    public DbSet<SearchProfile> SearchProfiles => Set<SearchProfile>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Occupation> Occupations => Set<Occupation>();
    public DbSet<Permit> Permits => Set<Permit>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PersonAdditionalInformation> PersonAdditionalInformation => Set<PersonAdditionalInformation>();
    public DbSet<Skills> Skills => Set<Skills>();
    public DbSet<WorkPreferences> WorkPreferences => Set<WorkPreferences>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor, new EncryptionInterceptor(Cryptor), new DecryptionInterceptor(Cryptor));
        optionsBuilder.ConfigureWarnings(warnings =>
        {
            warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning); // @TODO: Resolve
            //warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new PersonAdditionalInformationConfiguration());
        modelBuilder.ApplyConfiguration(new WorkPreferencesConfiguration());
        modelBuilder.ApplyConfiguration(new CertificationConfiguration());

        if (_isTesting) modelBuilder.ApplyConfiguration(new SearchProfileConfiguration());
    }
}
