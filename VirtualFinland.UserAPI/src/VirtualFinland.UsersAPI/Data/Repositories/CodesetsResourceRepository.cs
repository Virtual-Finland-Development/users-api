using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class CodesetsResourceRepository<T>
{
    private readonly CodesetsService _codesetsService;
    private ICacheRepository _cacheRepository;
    protected CodesetsResource? _resource;

    public CodesetsResourceRepository(CodesetsService codesetsService, ICacheRepositoryFactory cacheRepositoryFactory)
    {
        _codesetsService = codesetsService;
        _cacheRepository = cacheRepositoryFactory.Create("codesets");
    }

    public async Task<T> GetResource(CodesetsResource? resource = null)
    {
        var resourceToGet = resource ?? _resource ?? throw new Exception("Resource not defined");

        var cacheKey = resourceToGet.ToString();
        if (await _cacheRepository.Exists(cacheKey))
        {
            return await _cacheRepository.Get<T>(cacheKey);
        }

        var resolvedResource = await _codesetsService.GetResource<T>(resourceToGet);
        await _cacheRepository.Set<T>(cacheKey, resolvedResource);
        return resolvedResource;
    }
}