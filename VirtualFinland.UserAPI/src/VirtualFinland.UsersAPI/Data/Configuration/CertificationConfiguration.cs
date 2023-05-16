using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> entity)
    {
        entity.Property(c => c.EscoUri).HasConversion(
            v => v == null ? null : string.Join(',', v),
            v => v == null ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );
    }
}
