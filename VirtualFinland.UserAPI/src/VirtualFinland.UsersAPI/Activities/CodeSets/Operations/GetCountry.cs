using System.Globalization;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetCountry
{
    [SwaggerSchema(Title = "CountryCodeSetRequest")]
    public class Query : IRequest<Country>
    {
        public string ISOCode { get; }

        public Query(string isoCode)
        {
            this.ISOCode = isoCode;
        }
    }

    public class Handler : IRequestHandler<Query, Country>
    {

        public async Task<Country> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var country = CultureInfo.GetCultureInfo(request.ISOCode, true);
                return new Country(country.Name, country.DisplayName, country.EnglishName, country.NativeName, country.TwoLetterISOLanguageName, country.ThreeLetterISOLanguageName);
            }
            catch (CultureNotFoundException e)
            {
                throw new NotFoundException("Given culture not found");
            }
            
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string Nane, string DisplayName, string EnglishName, string NativeName, string TwoLetterISORegionName, string ThreeLetterISORegionName);
}

