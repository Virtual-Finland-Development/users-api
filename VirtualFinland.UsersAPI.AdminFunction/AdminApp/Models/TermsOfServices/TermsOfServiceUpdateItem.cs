namespace VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;

public class TermsOfServiceUpdateItem
{
    public string Url { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Version { get; set; } = default!;

    /// <summary>
    /// Special action to perform on the item
    /// </summary>
    public string? Action { get; set; }
}

