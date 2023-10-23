using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class LanguageRepository : CodesetsResourceRepository<List<Language>>, ILanguageRepository
{
    public LanguageRepository(CodesetsService codesetsService) : base(codesetsService)
    {
    }

    public Task<List<Language>> GetAllLanguages()
    {
        return GetResource();
    }
}