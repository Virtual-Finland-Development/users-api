using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public static class GetAllOccupations
{
    [SwaggerSchema(Title = "OccupationsCodeSetRequest")]
    public class Query : IRequest<List<Occupation>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Occupation>>
    {
        private readonly IOccupationsRepository _occupationsRepository;

        public Handler(IOccupationsRepository occupationsRepository)
        {
            _occupationsRepository = occupationsRepository;
        }
        public async Task<List<Occupation>> Handle(Query request, CancellationToken cancellationToken)
        {
            var occupationsRawData = await _occupationsRepository.GetAllOccupations();

            return occupationsRawData
                .Where( o => !string.IsNullOrEmpty(o.Notation))
                .Select(o => new Occupation(
                    o.Notation,
                    o.Uri,
                    new LanguageTranslations(
                        o.PrefLabel?.Finland,
                        o.PrefLabel?.English,
                        o.PrefLabel?.Swedish),
                    o.Narrower))
                .ToList();
        }
    }

    [SwaggerSchema(Title = "OccupationsCodeSetResponse")]
    public record Occupation(string? Notation, string? Uri, LanguageTranslations PrefLabel ,List<OccupationRoot.Occupation>? Narrower);

    [SwaggerSchema(Title = "OccupationLanguageTranslationsCodeSetResponse")]
    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

