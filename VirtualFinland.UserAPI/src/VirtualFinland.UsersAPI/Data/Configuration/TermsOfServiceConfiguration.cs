using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class TermsOfServiceConfiguration : IEntityTypeConfiguration<TermsOfService>
{
    public void Configure(EntityTypeBuilder<TermsOfService> entity)
    {
        entity.HasIndex(e => e.Version).IsUnique();
    }
}
