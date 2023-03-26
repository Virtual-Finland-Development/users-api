using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockCountriesRepository : ICountriesRepository
{

    public Task<List<Country>> GetAllCountries()
    {
        var countries = new List<Country>()
        {
            new Country()
            {
                IsoCode = "FI",
                IsoCodeThreeLetter = "FIN"
            },
            new Country()
            {
                IsoCode = "LA",
                IsoCodeThreeLetter = "LAO"
            },
            new Country()
            {
                IsoCode = "TV",
                IsoCodeThreeLetter = "TUV"
            },
        };

        return Task.FromResult<List<Country>>(countries);
    }
}
