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

        public Task<Country> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var country = new RegionInfo(request.ISOCode);
                return Task.FromResult(new Country(country.Name, country.Name, country.DisplayName, country.EnglishName, country.NativeName, country.TwoLetterISORegionName, country.ThreeLetterISORegionName));
            }
            catch (ArgumentException e)
            {
                throw new NotFoundException("Given culture not found", e);
            }
            
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string Id, string Nane, string DisplayName, string EnglishName, string NativeName, string TwoLetterISORegionName, string ThreeLetterISORegionName);
}

