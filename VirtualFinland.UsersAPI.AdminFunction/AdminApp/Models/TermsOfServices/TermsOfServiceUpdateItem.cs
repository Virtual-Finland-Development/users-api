using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;

public class TermsOfServiceUpdateItem
{
    public string Url { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Version { get; set; } = default!;

    /// <summary>
    /// Special action to perform on the item
    /// </summary>
    public TermsOfServiceUpdateItemAction? Action { get; set; }
}

[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum TermsOfServiceUpdateItemAction
{
    [EnumMember(Value = "UPDATE")] Update = 1,
    [EnumMember(Value = "DELETE")] Delete = 2,
}