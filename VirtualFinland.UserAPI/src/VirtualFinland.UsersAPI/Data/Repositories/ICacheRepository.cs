
namespace VirtualFinland.UserAPI.Data.Repositories;

public interface ICacheRepository
{
    Task<T> Get<T>(string key);
    Task Set<T>(string key, T value, TimeSpan? expiry = null);
    Task Remove(string key);
    Task<bool> Exists(string key);
    Task Clear();
}