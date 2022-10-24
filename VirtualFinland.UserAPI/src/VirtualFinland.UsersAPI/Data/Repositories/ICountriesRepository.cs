using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface ICountriesRepository
{
    Task<List<Country>> GetAllCountries();
}