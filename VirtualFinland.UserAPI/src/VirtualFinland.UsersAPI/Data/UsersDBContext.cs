using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UserAPI.Data;

public class UsersDbContext : DbContext
{
    protected override void OnConfiguring
        (DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "UsersDB");
    }
    public DbSet<User> Users { get; set; }
    
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; }
    
    public DbSet<SearchProfile> SearchProfiles { get; set; }
}