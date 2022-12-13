using System.IO.Compression;
using System.Text.Json;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsRepository : IOccupationsRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _occupationsUrl;
    private List<OccupationRoot.Occupation>? _occupations;

    public OccupationsRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _occupationsUrl = Environment.GetEnvironmentVariable("CODE_SET_OCCUPATIONS") ?? configuration["ExternalSources:OccupationsEscoURL"]; ;
        GetAllOccupations().Wait();
    }

    public async Task<List<OccupationRoot.Occupation>?> GetAllOccupations()
    {
        // TODO: Better cache control, maybe use .NET Core 6 In-Memory Cache. Fastest solution at the moment.
        if (_occupations is not null)
        {
            return _occupations;
        }

        var zipContents = await UnzipUrl(_occupationsUrl);

        if (!string.IsNullOrEmpty(zipContents))
        {
            var rootOccupationData = JsonSerializer.Deserialize<List<OccupationRoot.Occupation>>(zipContents);

            if (rootOccupationData is not null)
            {
                _occupations = rootOccupationData;
                return rootOccupationData;
            }
        }

        return new List<OccupationRoot.Occupation>();
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