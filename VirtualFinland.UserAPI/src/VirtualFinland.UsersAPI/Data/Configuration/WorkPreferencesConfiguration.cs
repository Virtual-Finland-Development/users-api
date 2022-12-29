using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class WorkPreferencesConfiguration : IEntityTypeConfiguration<WorkPreferences>
{
    public void Configure(EntityTypeBuilder<WorkPreferences> entity)
    {
        var municipalityConverter = new EnumCollectionJsonValueConverter<Municipality>();
        var municipalityComparer = new CollectionValueComparer<Municipality>();
        entity
            .Property(wp => wp.PreferredMunicipalityEnum)
            .HasConversion(municipalityConverter)
            .Metadata.SetValueComparer(municipalityComparer);

        var regionConverter = new EnumCollectionJsonValueConverter<Region>();
        var regionComparer = new CollectionValueComparer<Region>();

        entity
            .Property(wp => wp.PreferredRegionEnum)
            .HasConversion(regionConverter)
            .Metadata.SetValueComparer(regionComparer);
    }
}
