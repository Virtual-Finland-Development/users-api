using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class OccupationsBuilder
{
    private readonly DateTime _created = default;
    private readonly string? _escoCode = "005";
    private readonly string? _escoUri = "http://lmgtfy.com";
    private readonly Guid _id = Guid.Empty;
    private readonly DateTime _modified = default;
    private readonly string? _naceCode = "42.323";
    private readonly Guid _userId = Guid.Empty;
    private readonly int? _workMonths = 1;

    public Occupation Build()
    {
        return new Occupation
        {
            Id = _id,
            NaceCode = _naceCode,
            EscoUri = _escoUri,
            EscoCode = _escoCode,
            WorkMonths = _workMonths,
            Created = _created,
            Modified = _modified,
            PersonId = _userId
        };
    }
}
