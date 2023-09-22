
namespace VirtualFinland.UserAPI.Data.Repositories;

public interface ICacheRepositoryFactory
{
    ICacheRepository Create(string keyPrefix = "");
}