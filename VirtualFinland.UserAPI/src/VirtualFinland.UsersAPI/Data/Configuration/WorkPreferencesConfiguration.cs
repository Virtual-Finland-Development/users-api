using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class WorkPreferencesConfiguration : IEntityTypeConfiguration<WorkPreferences>
{
    public void Configure(EntityTypeBuilder<WorkPreferences> entity)
    {
        entity.Property(wp => wp.PreferredMunicipalityCode).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );

        entity.Property(wp => wp.PreferredRegionCode).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );

        entity.Property(wp => wp.WorkingLanguageCode).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );
    }
}
