using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> entity)
    {
        _ = entity.Property(c => c.EscoUri).HasConversion(
            v => string.Join(',', v ?? new List<string>()),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            new CommaSeparatedListComparator()
        );
    }
}
