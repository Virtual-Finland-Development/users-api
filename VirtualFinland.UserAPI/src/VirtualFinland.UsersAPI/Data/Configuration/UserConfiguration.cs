using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasData(new User
        {
            Id = UsersDbContext.WarmUpUserId,
            FirstName = "WarmUpUser",
            LastName = "WarmUpUser",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        });

        entity.Property(u => u.Gender).HasConversion<string>();

        entity.HasOne(u => u.WorkPreferences).WithOne(wp => wp.User).HasForeignKey<WorkPreferences>(wp => wp.Id);
    }
}
