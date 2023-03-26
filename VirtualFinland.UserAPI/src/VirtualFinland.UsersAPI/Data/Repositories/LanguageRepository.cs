using System.Text.Json;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _languagesUrl;
    private List<Language>? _languages;


    public LanguageRepository(IOptions<CodesetConfig> settings, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _languagesUrl = settings.Value.IsoLanguages;
    }

    public async Task<List<Language>> GetAllLanguages()
    {
        if (_languages is not null)
        {
            return _languages;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(_languagesUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var languages = JsonSerializer.Deserialize<List<Language>>(await httpResponseMessage.Content.ReadAsStringAsync());

            if (languages is not null)
            {
                _languages = languages;
                return languages;
            }
        }

        return new List<Language>();
    }
}