namespace VirtualFinland.UserAPI.Security.Models;

public class SecurityOptions
{
    public bool TermsOfServiceAgreementRequired { get; set; }
    public int ServiceRequestTimeoutInMilliseconds { get; set; }
}