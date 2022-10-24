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
            return countries.Select(o => new Country(o.id, o.DisplayName, o.EnglishName, o.NativeName, o.TwoLetterISORegionName, o.ThreeLetterISORegionName)).ToList();
        }
        
        public static List<RegionInfo> GetCountriesByIso3166()
        {
            List<RegionInfo> countries = new List<RegionInfo>();
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo country = new RegionInfo(culture.Name);
                if (countries.Count(p => p.Name == country.Name) == 0)
                    countries.Add(country);
            }
            return countries.OrderBy(p => p.EnglishName).ToList();
        }
    }

    [SwaggerSchema(Title = "CountryCodeSetResponse")]
    public record Country(string id, string DisplayName, string EnglishName, string NativeName, string TwoLetterISORegionName, string ThreeLetterISORegionName);
}

