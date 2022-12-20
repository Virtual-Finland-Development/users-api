using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public static class GetOccupation
{
    [SwaggerSchema(Title = "OccupationCodeSetRequest")]
    public class Query : IRequest<Occupation>
    {
        public string? Notation { get; }
        
        public Query(string? notation)
        {
            this.Notation = notation;
        }
    }

    public class Handler : IRequestHandler<Query, Occupation>
    {
        private readonly IOccupationsFlatRepository _occupationsFlatRepository;

        public Handler(IOccupationsFlatRepository occupationsFlatRepository)
        {
            _occupationsFlatRepository = occupationsFlatRepository;
        }

        public async Task<Occupation> Handle(Query request, CancellationToken cancellationToken)
        {
            var occupationsRawData = await _occupationsFlatRepository.GetAllOccupationsFlat();

            try
            {
                var occupationRaw = occupationsRawData.Single(o => o.Notation == request.Notation);

                return new Occupation(occupationRaw.Notation,
                    occupationRaw.Uri,
                    new LanguageTranslations(occupationRaw.PrefLabel?.Finland,
                        occupationRaw.PrefLabel?.English,
                        occupationRaw.PrefLabel?.Swedish),
                    occupationRaw.Broader);
            }
            catch (InvalidOperationException e)
            {
                throw new NotFoundException("Occupation was not found.", e);
            }
        }
    }

    [SwaggerSchema(Title = "OccupationCodeSetResponse")]
    public record Occupation(string? Notation, string? Uri, LanguageTranslations PrefLabel, List<string>? Broader);

    [SwaggerSchema(Title = "OccupationLanguageTranslationsCodeSetResponse")]
    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

