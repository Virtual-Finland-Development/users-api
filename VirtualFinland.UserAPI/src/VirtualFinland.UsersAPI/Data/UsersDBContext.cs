using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data.Configuration;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext, IDataProtectionKeyContext
{
    private readonly bool _isTesting;
    private readonly IAuditInterceptor _auditInterceptor;

    public UsersDbContext(DbContextOptions options, IAuditInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public UsersDbContext(DbContextOptions options, IAuditInterceptor auditInterceptor, bool isTesting) : base(options)
    {
        _isTesting = isTesting;
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
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
    public DbSet<TermsOfService> TermsOfServices => Set<TermsOfService>();
    public DbSet<PersonTermsOfServiceAgreement> PersonTermsOfServiceAgreements => Set<PersonTermsOfServiceAgreement>();
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new PersonAdditionalInformationConfiguration());
        modelBuilder.ApplyConfiguration(new WorkPreferencesConfiguration());
        modelBuilder.ApplyConfiguration(new CertificationConfiguration());
        modelBuilder.ApplyConfiguration(new TermsOfServiceConfiguration());

        if (_isTesting) modelBuilder.ApplyConfiguration(new SearchProfileConfiguration());
    }
}
