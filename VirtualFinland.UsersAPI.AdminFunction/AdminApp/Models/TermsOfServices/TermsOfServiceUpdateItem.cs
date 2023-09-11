using System.Runtime.Serialization;

namespace VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;

[DataContract]
public class TermsOfServiceUpdateItem
{
    [DataMember]
    public string Url { get; set; } = default!;

    [DataMember]
    public string Description { get; set; } = default!;

    [DataMember]
    public string Version { get; set; } = default!;

    /// <summary>
    /// Special action to perform on the item
    /// </summary>
    [DataMember]
    public SpecialActions? Action { get; set; }

    public enum SpecialActions { DELETE }
}

