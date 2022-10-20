using System.Text.Json.Nodes;
using VirtualFinland.UserAPI.Models.SuomiFi;

namespace VirtualFinland.UserAPI.Data;

public interface IOccupationsRepository
{
    Task<List<OccupationRoot.Occupation>?> GetAllOccupationsRaw();
}