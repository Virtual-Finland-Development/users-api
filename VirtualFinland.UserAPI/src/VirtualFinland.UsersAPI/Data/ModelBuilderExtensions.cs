using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data;

public static class ModelBuilderExtensions
{
    /// <summary>
    ///     This is an extension method method used for seeding database with initial data
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasData(new Person
        {
            Id = WarmUpUser.Id,
            GivenName = WarmUpUser.GivenName,
            LastName = WarmUpUser.LastName,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        });

        modelBuilder.Entity<ExternalIdentity>().HasData(new ExternalIdentity
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            UserId = WarmUpUser.Id,
            IdentityId = Guid.NewGuid().ToString(),
            Issuer = Guid.NewGuid().ToString()
        });
    }
}
