using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// </summary>
public class InvalidateCachesAction : IAdminAppAction
{
    private readonly ICacheRepositoryFactory _cacheRepositoryFactory;
    private readonly ILogger<InvalidateCachesAction> _logger;

    public InvalidateCachesAction(ICacheRepositoryFactory cacheRepositoryFactory, ILogger<InvalidateCachesAction> logger)
    {
        _cacheRepositoryFactory = cacheRepositoryFactory;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        _logger.LogInformation("Invalidating caches");

        // Invalidate all caches
        var cacheRepository = _cacheRepositoryFactory.Create();
        await cacheRepository.Clear();

        _logger.LogInformation("Caches invalidated");
    }
}