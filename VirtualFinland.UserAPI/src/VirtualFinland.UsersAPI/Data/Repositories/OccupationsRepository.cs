using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class OccupationsRepository : CodesetsResourceRepository<List<OccupationRoot.Occupation>>, IOccupationsRepository
{
    public OccupationsRepository(CodesetsService codesetsService) : base(codesetsService)
    {
        _resource = CodesetsResource.Occupations;
    }

    public Task<List<OccupationRoot.Occupation>> GetAllOccupations() => GetResource();
}