using System.Globalization;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CountriesRepository : ICountriesRepository
{

    public Task<List<Country>> GetAllCountries()
    {
        List<RegionInfo> countries = new List<RegionInfo>();
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            RegionInfo country = new RegionInfo(culture.Name);
            if (countries.Count(p => p.Name == country.Name) == 0)
                countries.Add(country);
        }
        return Task.FromResult(countries.OrderBy(p => p.EnglishName).Select(o => new Country(o.Name,o.DisplayName, o.EnglishName, o.NativeName, o.TwoLetterISORegionName, o.ThreeLetterISORegionName)).ToList());
    }
}