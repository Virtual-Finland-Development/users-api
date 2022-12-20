using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockOccupationsRepository : IOccupationsFlatRepository
{

    public Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat()
    {
        var occupations = new List<OccupationFlatRoot.Occupation>()
        {
            new OccupationFlatRoot.Occupation() { Notation = "01"},
            new OccupationFlatRoot.Occupation() { Notation = "02"},
            new OccupationFlatRoot.Occupation() { Notation = "03"}
        };
        
        return Task.FromResult<List<OccupationFlatRoot.Occupation>>(occupations);
    }
}