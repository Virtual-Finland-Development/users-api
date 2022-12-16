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
        private readonly IOccupationsRepository _occupationsRepository;

        public Handler(IOccupationsRepository occupationsRepository)
        {
            _occupationsRepository = occupationsRepository;
        }

        public async Task<Occupation> Handle(Query request, CancellationToken cancellationToken)
        {
            var occupationsRawData = await _occupationsRepository.GetAllOccupations();

            try
            {
                List<OccupationRoot.Occupation> occupationsFlattened = new();

                foreach (var occupation in occupationsRawData)
                {
                    FlattenOccupations(occupation, occupationsFlattened);
                }

                var occupationRaw = occupationsFlattened.Single(o => o.Notation == request.Notation);

                return new Occupation(occupationRaw.Notation,
                    occupationRaw.Uri,
                    new LanguageTranslations(occupationRaw.PrefLabel?.Finland,
                        occupationRaw.PrefLabel?.English,
                        occupationRaw.PrefLabel?.Swedish),
                    occupationRaw.Narrower);
            }
            catch (InvalidOperationException e)
            {
                throw new NotFoundException("Occupation was not found.", e);
            }
        }

        void FlattenOccupations(OccupationRoot.Occupation occupation, List<OccupationRoot.Occupation> occupations)
        {
            occupations.Add(occupation);

            if (occupation.Narrower != null)
            {
                foreach (var childOccupation in occupation.Narrower)
                {
                    FlattenOccupations(childOccupation, occupations);
                }
            }
        }
    }

    [SwaggerSchema(Title = "OccupationCodeSetResponse")]
    public record Occupation(string? Notation, string? Uri, LanguageTranslations PrefLabel, List<OccupationRoot.Occupation>? Narrower);

    [SwaggerSchema(Title = "OccupationLanguageTranslationsCodeSetResponse")]
    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

