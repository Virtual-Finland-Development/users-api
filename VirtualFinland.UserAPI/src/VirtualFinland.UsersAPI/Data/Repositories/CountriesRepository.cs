using System.Text.Json;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CountriesRepository : ICountriesRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _iso3166CountriesUrl;
    private List<Country>? _countries;

    public CountriesRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _iso3166CountriesUrl = Environment.GetEnvironmentVariable("CODE_SET_COUNTRIES") ?? configuration["ExternalSources:ISO3166CountriesURL"];
        GetAllCountries().Wait();
    }
    public async Task<List<Country>> GetAllCountries()
    {
        if (_countries is not null)
        {
            return _countries;
        }
        
        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(_iso3166CountriesUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var countries = JsonSerializer.Deserialize<List<Country>>(await httpResponseMessage.Content.ReadAsStringAsync());

            if (countries is not null)
            {
                _countries = countries;
                return _countries;
            }
        }

        return new List<Country>();
    }
}