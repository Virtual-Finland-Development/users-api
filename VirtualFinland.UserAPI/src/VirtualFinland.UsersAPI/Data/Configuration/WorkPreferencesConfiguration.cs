using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class WorkPreferencesConfiguration : IEntityTypeConfiguration<WorkPreferences>
{
    public void Configure(EntityTypeBuilder<WorkPreferences> entity)
    {
        entity.Property(wp => wp.PreferredMunicipalityCode).HasConversion(
            v => v == null ? null : string.Join(',', v),
            v => v == null ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );

        entity.Property(wp => wp.PreferredRegionCode).HasConversion(
            v => v == null ? null : string.Join(',', v),
            v => v == null ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );

        entity.Property(wp => wp.WorkingLanguageEnum).HasConversion(
            v => v == null ? null : string.Join(',', v),
            v => v == null ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );
    }
}
