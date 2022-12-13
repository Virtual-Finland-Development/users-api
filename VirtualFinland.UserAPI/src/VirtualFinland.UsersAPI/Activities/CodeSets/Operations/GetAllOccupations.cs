using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;

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

            return occupationsRawData?.Where( o => int.TryParse(o.Id, out _)).Select(o => new Occupation(o.Id,
                    o.Uri,
                    new LanguageTranslations(o.Name?.Finland,
                        o.Name?.English,
                        o.Name?.Swedish),
                    o.Broader))
                .OrderBy(o=> int.Parse(o.Id!)).ToList() ?? new List<Occupation>();
        }
    }

    [SwaggerSchema(Title = "OccupationsCodeSetResponse")]
    public record Occupation(string? Id, string? Uri, LanguageTranslations Name, List<string>? Broader);

    [SwaggerSchema(Title = "OccupationLanguageTranslationsCodeSetResponse")]
    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

