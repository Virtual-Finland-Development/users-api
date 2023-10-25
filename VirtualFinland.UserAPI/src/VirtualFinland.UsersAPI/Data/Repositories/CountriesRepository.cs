using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CountriesRepository : CodesetsResourceRepository<List<Country>>, ICountriesRepository
{
    public CountriesRepository(CodesetsService codesetsService) : base(codesetsService)
    {
        _resource = CodesetsResource.Countries;
    }

    public Task<List<Country>> GetAllCountries() => GetResource();
}