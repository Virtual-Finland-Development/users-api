using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class CodesetsResourceRepository<T>
{
    private readonly CodesetsService _codesetsService;
    private T? _resourceCache;
    protected CodesetConfig.Resource? _resource;

    public CodesetsResourceRepository(CodesetsService codesetsService)
    {
        _codesetsService = codesetsService;
    }

    public async Task<T> GetResource(CodesetConfig.Resource? resource = null)
    {
        if (_resourceCache is not null)
        {
            return _resourceCache;
        }

        var resourceToGet = resource ?? _resource ?? throw new Exception("Resource not defined");
        _resourceCache = await _codesetsService.GetResource<T>(resourceToGet);
        return _resourceCache;
    }
}