using System.Collections;
using MediatR;
using VirtualFinland.UserAPI.Data;
using System.Linq;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.SuomiFi;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetOccupation
{
    public class Query : IRequest<Occupation>
    {
        public string? ISCOCode { get; }
        
        public Query(string? iscoCode)
        {
            this.ISCOCode = iscoCode;
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
            var occupationsRawData = await _occupationsRepository.GetAllOccupationsRaw();

            try
            {
                var occupationRaw = occupationsRawData?.Single(o => o.Id == request.ISCOCode);

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

    public record Occupation(string? Id, LanguageTranslations Name, LanguageTranslations Description);

    public record LanguageTranslations(string? Finland, string? English, string? Swedish);
}

