using System.Text.Json;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsRepository : IOccupationsRepository
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _codeSetsSuomiFiUrl;
    private List<OccupationRoot.Occupation>? _occupations;
    public OccupationsRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _codeSetsSuomiFiUrl = _configuration["ExternalSources:CodeSetsSuomiFiURL"];
    }
    
    public async Task<List<OccupationRoot.Occupation>?> GetAllOccupations()
    {
        // TODO: Better cache control, maybe use .NET Core 6 In-Memory Cache. Fastest solution at the moment.
        if (_occupations is not null)
        {
            return _occupations;
        }
        
        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(_codeSetsSuomiFiUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var rootOccupationData = JsonSerializer.Deserialize<OccupationRoot>(await httpResponseMessage.Content.ReadAsStringAsync());

            if (rootOccupationData is not null)
            {
                _occupations = rootOccupationData.Occupations;
                return rootOccupationData.Occupations;
            }
        }

        return new List<OccupationRoot.Occupation>();
    }
}