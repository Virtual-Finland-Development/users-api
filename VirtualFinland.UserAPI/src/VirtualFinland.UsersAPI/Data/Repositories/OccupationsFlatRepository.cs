using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsFlatRepository : CodesetsResourceRepository<List<OccupationFlatRoot.Occupation>>, IOccupationsFlatRepository
{
    public OccupationsFlatRepository(CodesetsService codesetsService) : base(codesetsService)
    {
    }

    public Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat()
    {
        return GetResource();
    }
}