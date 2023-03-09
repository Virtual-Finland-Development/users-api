using System.Text.Json;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsRepository : IOccupationsRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _occupationsUrl;
    private List<OccupationRoot.Occupation>? _occupations;

    public OccupationsRepository(IOptions<CodesetConfig> settings, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _occupationsUrl = settings.Value.OccupationsEscoUrl;
    }

    public async Task<List<OccupationRoot.Occupation>> GetAllOccupations()
    {
        // TODO: Better cache control, maybe use .NET Core 6 In-Memory Cache. Fastest solution at the moment.
        if (_occupations is not null)
        {
            return _occupations;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(_occupationsUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var rootOccupationData = JsonSerializer.Deserialize<List<OccupationRoot.Occupation>>(await httpResponseMessage.Content.ReadAsStringAsync());

            if (rootOccupationData is not null)
            {
                _occupations = rootOccupationData;
                return rootOccupationData;
            }
        }

        return new List<OccupationRoot.Occupation>();
    }
}