using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext
{
    private readonly bool _isTesting = false;
    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public UsersDbContext(DbContextOptions options, bool isTesting) : base(options)
    {
        _isTesting = isTesting;
    }
    protected override void OnConfiguring
        (DbContextOptionsBuilder optionsBuilder)
    {
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
    }
    public DbSet<User> Users { get; set; }
    
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; }
    
    public DbSet<SearchProfile> SearchProfiles { get; set; }
}