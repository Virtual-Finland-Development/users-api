using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data.Configuration;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext
{
    private readonly bool _isTesting;

    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }

    public UsersDbContext(DbContextOptions options, bool isTesting) : base(options)
    {
        _isTesting = isTesting;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

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
