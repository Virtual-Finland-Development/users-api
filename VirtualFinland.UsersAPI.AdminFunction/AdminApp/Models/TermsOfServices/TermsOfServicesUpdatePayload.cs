namespace VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;

public class TermsOfServicesUpdatePayload
{
    public List<TermsOfServiceUpdateItem> TermsOfServices { get; set; } = new();
}