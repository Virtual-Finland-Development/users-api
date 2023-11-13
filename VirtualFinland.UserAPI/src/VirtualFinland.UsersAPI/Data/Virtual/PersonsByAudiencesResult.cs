
using Microsoft.EntityFrameworkCore;

namespace VirtualFinland.UserAPI.Data.Virtual;

[Keyless]
public class PersonsByAudiencesResult
{
    public string? Audience { get; set; }
    public int Amount { get; set; }
}