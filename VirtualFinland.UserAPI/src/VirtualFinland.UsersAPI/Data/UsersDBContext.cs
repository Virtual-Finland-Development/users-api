using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext
{

    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnConfiguring
        (DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseInMemoryDatabase(databaseName: "UsersDB");
        optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=example");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        /*modelBuilder.Entity<SearchProfile>()
            .Property(e => e.JobTitles)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        
        modelBuilder.Entity<SearchProfile>()
            .Property(e => e.Regions)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());*/
    }
    public DbSet<User> Users { get; set; }
    
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; }
    
    public DbSet<SearchProfile> SearchProfiles { get; set; }
}