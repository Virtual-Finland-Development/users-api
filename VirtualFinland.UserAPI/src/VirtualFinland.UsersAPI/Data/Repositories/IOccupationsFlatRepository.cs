using VirtualFinland.UserAPI.Models.Repositories;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface IOccupationsFlatRepository
{
    Task<List<OccupationFlatRoot.Occupation>> GetAllOccupationsFlat();
}

