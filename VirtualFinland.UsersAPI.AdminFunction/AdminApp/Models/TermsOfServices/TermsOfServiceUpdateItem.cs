namespace VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;

public record TermsOfServiceUpdateItem
{
    public string Url { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Version { get; set; } = default!;
}