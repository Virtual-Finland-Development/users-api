using System.Globalization;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetAllLanguages
{
    [SwaggerSchema(Title = "LanguagesCodeSetRequest")]
    public class Query : IRequest<List<Language>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Language>>
    {

        public Task<List<Language>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(o => new Language(o.Name, o.DisplayName, o.EnglishName, o.NativeName, o.TwoLetterISOLanguageName, o.ThreeLetterISOLanguageName)).ToList());
        }
        

    }

    [SwaggerSchema(Title = "LanguageCodeSetResponse")]
    public record Language(string id, string DisplayName, string EnglishName, string NativeName, string TwoLetterISORegionName, string ThreeLetterISORegionName);
}

