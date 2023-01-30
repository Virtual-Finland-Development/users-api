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

    public string? SkillLevelEnum { get; set; }

    [JsonIgnore]
    public Person Person { get; set; } = null!;

    public Guid Id { get; set; }
}
