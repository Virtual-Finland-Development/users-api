using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface ILanguageRepository
{
    Task<List<Language>> GetAllLanguages();
}