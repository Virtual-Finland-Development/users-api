using VirtualFinland.UserAPI.Models.Repositories;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface ITermsOfServiceRepository
{
    /// <summary>
    /// Get newest terms of service
    /// </summary>
    Task<TermsOfService> GetNewestTermsOfService();

    /// <summary>
    /// Get terms of service by version
    /// </summary>
    Task<TermsOfService?> GetTermsOfServiceByVersion(string version);

    /// <summary>
    /// Get newest terms of service agreement by person id
    /// </summary>
    Task<PersonTermsOfServiceAgreement?> GetNewestTermsOfServiceAgreementByPersonId(Guid personId);
}

