using Microsoft.EntityFrameworkCore;
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

    public static Guid WarmUpUserId => Guid.Parse("5a8af4b4-8cb4-44ac-8291-010614601719");

    public DbSet<User> Users => Set<User>();
    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
    public DbSet<SearchProfile> SearchProfiles => Set<SearchProfile>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Method intentionally left empty.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (_isTesting)
        {
            modelBuilder.Entity<SearchProfile>()
                .Property(e => e.JobTitles)
                .HasConversion(
                    v => string.Join(',', v!),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            modelBuilder.Entity<SearchProfile>()
                .Property(e => e.Regions)
                .HasConversion(
                    v => string.Join(',', v!),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        }
        
        modelBuilder.Entity<ExternalIdentity>().HasData(new ExternalIdentity
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            UserId = WarmUpUserId,
            IdentityId = Guid.NewGuid().ToString(),
            Issuer = Guid.NewGuid().ToString()
        });
    }
}
