using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UserAPI.Security.Models;

public class SecurityClientProviders
{
    public HttpClient HttpClient = default!;
    public ICacheRepositoryFactory CacheRepositoryFactory = default!;
}