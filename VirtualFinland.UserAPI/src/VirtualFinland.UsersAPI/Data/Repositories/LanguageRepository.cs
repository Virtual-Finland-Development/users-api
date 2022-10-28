using System.Globalization;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class LanguageRepository : ILanguageRepository
{

    public Task<List<Language>> GetAllLanguages()
    {
        return Task.FromResult(CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(o => !string.IsNullOrEmpty(o.Name))
            .Select(o => new Language(o.Name,
                o.DisplayName,
                o.EnglishName,
                o.NativeName,
                o.TwoLetterISOLanguageName,
                o.ThreeLetterISOLanguageName))
            .ToList());
    }
}