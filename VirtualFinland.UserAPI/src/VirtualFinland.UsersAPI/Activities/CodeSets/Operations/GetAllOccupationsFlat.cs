using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public static class GetAllOccupationsFlat
{
    [SwaggerSchema(Title = "OccupationsCodeSetFlatRequest")]
    public class Query : IRequest<List<Occupation>>
    {

    }

    public class Handler : IRequestHandler<Query, List<Occupation>>
    {
        private readonly IOccupationsFlatRepository _occupationsFlatRepository;

        public Handler(IOccupationsFlatRepository occupationsFlatRepository)
        {
            _occupationsFlatRepository = occupationsFlatRepository;
        }
        public async Task<List<Occupation>> Handle(Query request, CancellationToken cancellationToken)
        {
            var occupationsRawData = await _occupationsFlatRepository.GetAllOccupationsFlat();

            return occupationsRawData
                .Where(o => !string.IsNullOrEmpty(o.Notation))
                .Select(o => new Occupation(
                    o.Notation, 
                    o.Uri,
                    new LanguageTranslations(
                        o.PrefLabel?.Finland,
                        o.PrefLabel?.English,
                        o.PrefLabel?.Swedish),
                    o.Broader))
                .ToList();
        }
    }

    [SwaggerSchema(Title = "OccupationsCodeSetFlatResponse")]
    public record Occupation(string? Notation, string? Uri, LanguageTranslations PrefLabel, List<string>? Broader);

    [SwaggerSchema(Title = "OccupationLanguageTranslationsCodeSetFlatResponse")]
    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

