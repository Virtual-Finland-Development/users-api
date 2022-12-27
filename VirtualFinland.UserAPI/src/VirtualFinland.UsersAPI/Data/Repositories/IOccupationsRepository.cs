using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface IOccupationsRepository
{
    Task<List<OccupationRoot.Occupation>> GetAllOccupations();
}