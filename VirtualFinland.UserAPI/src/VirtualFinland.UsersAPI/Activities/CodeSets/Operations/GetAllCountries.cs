using System.Globalization;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetAllCountries
{
    [SwaggerSchema(Title = "CountryCodeSetRequest")]
    public class Query : IRequest<List<Country>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Country>>
    {

        public async Task<List<Country>> Handle(Query request, CancellationToken cancellationToken)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Select(o => new Country(o.Name, o.DisplayName, o.EnglishName, o.NativeName, o.TwoLetterISOLanguageName, o.ThreeLetterISOLanguageName))
                .ToList();
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string Nane, string DisplayName, string EnglishName, string NativeName, string TwoLetterISORegionName, string ThreeLetterISORegionName);
}

