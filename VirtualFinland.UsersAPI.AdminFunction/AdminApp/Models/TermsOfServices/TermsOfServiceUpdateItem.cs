using System.Runtime.Serialization;

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

public enum TermsOfServiceUpdateItemAction
{
    [EnumMember(Value = "UPDATE")] Update = 1,
    [EnumMember(Value = "DELETE")] Delete = 2,
}