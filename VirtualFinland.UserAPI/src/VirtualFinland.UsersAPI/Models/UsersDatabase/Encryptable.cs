using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public abstract class Encryptable : IEncrypted
{
    [NotMapped]
    public string? DataAccessKey { get; set; }

    /// <summary>
    ///  Implement the IClonable by deep cloning with JSON serialization
    /// </summary>
    public object Clone()
    {
        return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this), this.GetType()) ?? throw new InvalidOperationException("Could not clone object");
    }
}
