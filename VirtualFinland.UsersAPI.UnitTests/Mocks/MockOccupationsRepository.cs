using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Mocks;

public class MockOccupationsRepository : IOccupationsRepository
{

    public Task<List<OccupationRoot.Occupation>?> GetAllOccupations()
    {
        var occupations = new List<OccupationRoot.Occupation>()
        {
            new OccupationRoot.Occupation() { Id = "01"},
            new OccupationRoot.Occupation() { Id = "02"},
            new OccupationRoot.Occupation() { Id = "03"}
        };
        
        return Task.FromResult<List<OccupationRoot.Occupation>?>(occupations);
    }
}