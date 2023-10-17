using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockOccupationsRepository : IOccupationsFlatRepository
{

    public Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat()
    {
        var occupations = new List<OccupationFlatRoot.Occupation>()
        {
            new() { Notation = "01"},
            new() { Notation = "02"},
            new() { Notation = "03"},
            new() { Notation = "2654.1.7"},
        };

        return Task.FromResult<List<OccupationFlatRoot.Occupation>>(occupations);
    }
}