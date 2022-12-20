using System.IO.Compression;
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
        _occupationsFlatUrl = Environment.GetEnvironmentVariable("CODE_SET_OCCUPATIONS_FLAT") ?? configuration["ExternalSources:OccupationsFlatURL"]; ;
        GetAllOccupationsFlat().Wait();
    }

    public async Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat()
    {
        // TODO: Better cache control, maybe use .NET Core 6 In-Memory Cache. Fastest solution at the moment.
        if (_occupationsFlat is not null)
        {
            return _occupationsFlat;
        }

        var zipContents = await UnzipUrl(_occupationsFlatUrl);

        if (!string.IsNullOrEmpty(zipContents))
        {
            var rootOccupationFlatData = JsonSerializer.Deserialize<List<OccupationFlatRoot.Occupation>>(zipContents);

            if (rootOccupationFlatData is not null)
            {
                _occupationsFlat = rootOccupationFlatData;
                return rootOccupationFlatData;
            }
        }

        return new List<OccupationFlatRoot.Occupation>();
    }

    /// <summary>
    /// Downloads a zip file from the given url and returns the content of the first file in the zip archive.
    /// </summary>
    async Task<string> UnzipUrl(string zipUrl)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.GetAsync(zipUrl);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = zipArchive.Entries.FirstOrDefault();

            if (entry is not null)
            {
                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream);
                return await reader.ReadToEndAsync();
            }
        }

        return string.Empty;
    }
}
