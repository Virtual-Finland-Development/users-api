using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

[Keyless]
[NotMapped]
public class AuditableMetadata
{
    public AuditableMetadata()
    {
    }

    public AuditableMetadata(IRequestAuthenticationCandinate user)
    {
        TraceId = user.TraceId;
        Issuer = user.Issuer;
        Audience = user.Audience;
    }

    public string? TraceId { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}