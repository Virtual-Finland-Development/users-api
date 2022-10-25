using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public static class GetCountry
{
    [SwaggerSchema(Title = "CountryCodeSetRequest")]
    public class Query : IRequest<Country>
    {
        public string Id { get; }

        public Query(string id)
        {
            this.Id = id;
        }
    }

    public class Handler : IRequestHandler<Query, Country>
    {
        private readonly ICountriesRepository _countriesRepository;
        public Handler(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }
        public async Task<Country> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var countries = await _countriesRepository.GetAllCountries();
                var country = countries.Single(o => o.IsoCode == request.Id);
                return new Country(country.IsoCode, country.Name?.Common, country.Name?.Common, String.Empty, country.IsoCode, country.IsoCodeTßhreeLetter);
            }
            catch (InvalidOperationException e)
            {
                throw new NotFoundException("Given culture not found", e);
            }
            
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string? Id, string? DisplayName, string? EnglishName, string? NativeName, string? TwoLetterISORegionName, string? ThreeLetterISORegionName);
}

