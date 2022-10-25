using System.Collections;
using MediatR;
using VirtualFinland.UserAPI.Data;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetOccupation
{
    [SwaggerSchema(Title = "OccupationCodeSetRequest")]
    public class Query : IRequest<Occupation>
    {
        public string? Id { get; }
        
        public Query(string? id)
        {
            this.Id = id;
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
                var occupationRaw = occupationsRawData?.Single(o => o.Id == request.Id);

                return new Occupation(occupationRaw?.Id,
                    new LanguageTranslations(occupationRaw?.Name?.Finland,
                        occupationRaw?.Name?.English,
                        occupationRaw?.Name?.Swedish),
                    new LanguageTranslations(occupationRaw?.Description?.Finland,
                        occupationRaw?.Description?.English,
                        occupationRaw?.Description?.Swedish));
            }
            catch (InvalidOperationException e)
            {
                throw new NotFoundException("Occupation was not found.", e);
            }
        }
    }

    [SwaggerSchema(Title = "OccupationCodeSetResponse")]
    public record Occupation(string? Id, LanguageTranslations Name, LanguageTranslations Description);

    public record LanguageTranslations(string? Fi, string? En, string? Sw);
}

