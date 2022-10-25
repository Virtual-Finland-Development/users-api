using System.Globalization;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetAllLanguages
{
    [SwaggerSchema(Title = "LanguagesCodeSetRequest")]
    public class Query : IRequest<List<Language>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Language>>
    {
        private readonly ILanguageRepository _languageRepository;

        public Handler(ILanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }
        public async Task<List<Language>> Handle(Query request, CancellationToken cancellationToken)
        {
            var languages= await _languageRepository.GetAllLanguages();
            return languages.Select(o => new Language(o.Id, o.DisplayName, o.EnglishName, o.NativeName, o.TwoLetterISOLanguageName, o.ThreeLetterISOLanguageName)).ToList();
        }
        

    }

    [SwaggerSchema(Title = "LanguageCodeSetResponse")]
    public record Language(string id, string DisplayName, string EnglishName, string NativeName, string TwoLetterISOLanguageName, string ThreeLetterISOLanguageName);
}

