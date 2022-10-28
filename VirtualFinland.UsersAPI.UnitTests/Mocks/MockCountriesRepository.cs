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
                ISOCode = "FI",
                ISOCodeThreeLetter = "FIN"
            },
            new Country()
            {
                Name = new Country.CountryName()
                {
                    Common = "Laos"
                },
                ISOCode = "LA",
                ISOCodeThreeLetter = "LAO"
            },
            new Country()
            {
                Name = new Country.CountryName()
                {
                    Common = "Tuvalu"
                },
                ISOCode = "TV",
                ISOCodeThreeLetter = "TUV"
            },
        };
        
        return Task.FromResult<List<Country>>(countries);
    }
}