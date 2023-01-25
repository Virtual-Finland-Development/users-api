using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Skills : Auditable, IEntity
{
    public enum SkillLevel
    {
        Beginner,
        Intermediate,
        Master
    }

    [Url]
    public string? EscoUri { get; set; }

    public SkillLevel SkillLevelEnum { get; set; }

    [JsonIgnore]
    public Person Person { get; set; } = null!;

    [JsonIgnore]
    public Guid? EducationId { get; set; }

    [JsonIgnore]
    public Guid? OccupationId { get; set; }

    public Guid Id { get; set; }
}
