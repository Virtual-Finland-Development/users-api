using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class CodesetsResourceRepository<T>
{
    private readonly CodesetsService _codesetsService;
    private T? _resource;

    public CodesetsResourceRepository(CodesetsService codesetsService)
    {
        _codesetsService = codesetsService;
    }

    public async Task<T> GetResource()
    {
        if (_resource is not null)
        {
            return _resource;
        }

        var resource = await _codesetsService.GetResource<T>();

        if (_resource is not null)
        {
            _resource = resource;
            return _resource;
        }

        throw new Exception("Resource not found");
    }
}