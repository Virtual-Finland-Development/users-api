using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockLanguageRepository : ILanguageRepository
{

    public Task<List<Language>> GetAllLanguages()
    {
        var languages = new List<Language>()
        {
            new Language()
            {
                Id = "fail"
            },
            new Language()
            {
                Id = "swe"
            },
            new Language()
            {
                Id = "eng"
            },
        };

        return Task.FromResult<List<Language>>(languages);
    }

}
