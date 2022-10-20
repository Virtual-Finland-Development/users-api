using System.Collections;
using MediatR;
using VirtualFinland.UserAPI.Data;
using System.Linq;
using VirtualFinland.UserAPI.Models.SuomiFi;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public class GetAllOccupations
{
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
            var occupationsRawData = await _occupationsRepository.GetAllOccupationsRaw();

            return occupationsRawData?.Where( o => int.TryParse(o.Id, out _)).Select(o => new Occupation(o.Id,
                    new LanguageTranslations(o.Name?.Finland,
                        o.Name?.English,
                        o.Name?.Swedish),
                    new LanguageTranslations(o.Description?.Finland,
                        o.Description?.English,
                        o.Description?.Swedish)))
                .OrderBy(o=> int.Parse(o.Id!)).ToList() ?? new List<Occupation>();
        }
    }

    public record Occupation(string? Id, LanguageTranslations Name, LanguageTranslations Description);

    public record LanguageTranslations(string? Finland, string? English, string? Swedish);
}
