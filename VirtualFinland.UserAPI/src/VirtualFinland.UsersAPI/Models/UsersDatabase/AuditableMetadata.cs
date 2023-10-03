namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class AuditableMetadata
{
    public string? TraceId { get; set; }
    public string? IdentityId { get; set; }
    public string? IdentityIssuer { get; set; }
    public string? IdentityAudience { get; set; }
}