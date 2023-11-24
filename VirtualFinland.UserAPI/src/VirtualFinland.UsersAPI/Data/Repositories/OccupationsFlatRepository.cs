using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsFlatRepository : CodesetsResourceRepository<List<OccupationFlatRoot.Occupation>>, IOccupationsFlatRepository
{
    public OccupationsFlatRepository(CodesetsService codesetsService, ICacheRepositoryFactory cacheRepositoryFactory) : base(codesetsService, cacheRepositoryFactory)
    {
        _resource = CodesetsResource.OccupationsFlat;
    }

    public Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat() => GetResource();
}