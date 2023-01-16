using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class OccupationConfiguration : IEntityTypeConfiguration<Occupation>
{
    public void Configure(EntityTypeBuilder<Occupation> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasAlternateKey(o => new { o.UserId, o.EscoUri, o.EscoCode });
    }
}
