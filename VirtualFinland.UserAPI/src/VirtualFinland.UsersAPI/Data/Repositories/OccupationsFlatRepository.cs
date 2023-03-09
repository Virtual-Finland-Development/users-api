using System.Text.Json;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsFlatRepository : IOccupationsFlatRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _occupationsFlatUrl;
    private List<OccupationFlatRoot.Occupation>? _occupationsFlat;

    public OccupationsFlatRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _occupationsFlatUrl = configuration["ExternalSources:OccupationsFlatURL"];
    }

    public async Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat()
    {
        // TODO: Better cache control, maybe use .NET Core 6 In-Memory Cache. Fastest solution at the moment.
        if (_occupationsFlat is not null)
        {
            return _occupationsFlat;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(_occupationsFlatUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var rootOccupationFlatData = JsonSerializer.Deserialize<List<OccupationFlatRoot.Occupation>>(await httpResponseMessage.Content.ReadAsStringAsync());

            if (rootOccupationFlatData is not null)
            {
                _occupationsFlat = rootOccupationFlatData;
                return rootOccupationFlatData;
            }
        }

        return new List<OccupationFlatRoot.Occupation>();
    }
}
