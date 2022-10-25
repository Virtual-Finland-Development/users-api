using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockContriesRepository : ICountriesRepository
{

    public Task<List<Country>> GetAllCountries()
    {
        var countries = new List<Country>()
        {
            new Country()
            {
                Name = new Country.CountryName()
                {
                    Common = "Finland"
                },
                IsoCode = "FI",
                IsoCodeTßhreeLetter = "FIN"
            },
            new Country()
            {
                Name = new Country.CountryName()
                {
                    Common = "Laos"
                },
                IsoCode = "LA",
                IsoCodeTßhreeLetter = "LAO"
            },
            new Country()
            {
                Name = new Country.CountryName()
                {
                    Common = "Tuvalu"
                },
                IsoCode = "TV",
                IsoCodeTßhreeLetter = "TUV"
            },
        };
        
        return Task.FromResult<List<Country>>(countries);
    }
}