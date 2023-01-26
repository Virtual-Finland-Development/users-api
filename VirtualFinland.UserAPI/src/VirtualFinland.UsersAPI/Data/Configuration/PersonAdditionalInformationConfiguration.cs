using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Configuration;

public class PersonAdditionalInformationConfiguration : IEntityTypeConfiguration<PersonAdditionalInformation>
{
    public void Configure(EntityTypeBuilder<PersonAdditionalInformation> entity)
    {
        entity.HasOne(e => e.Address).WithOne().HasForeignKey<Address>(a => a.Id);
        entity.ToTable("PersonAdditionalInformation");
    }
}
