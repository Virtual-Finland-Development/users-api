using System.Globalization;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetAllCountries
{
    [SwaggerSchema(Title = "CountryCodeSetRequest")]
    public class Query : IRequest<List<Country>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Country>>
    {
        private readonly ICountriesRepository _countriesRepository;

        public Handler(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }
        public async Task<List<Country>> Handle(Query request, CancellationToken cancellationToken)
        {
            var countries = await _countriesRepository.GetAllCountries();
            return countries.Select(o => new Country(o.IsoCode, o?.Name?.Common, o?.Name?.Common, String.Empty, o?.IsoCode, o?.IsoCodeTßhreeLetter)).ToList();
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string? id, string? DisplayName, string? EnglishName, string? NativeName, string? TwoLetterISORegionName, string? ThreeLetterISORegionName);
}

