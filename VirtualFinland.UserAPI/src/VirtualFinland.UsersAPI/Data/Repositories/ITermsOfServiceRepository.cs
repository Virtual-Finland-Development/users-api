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
    /// Get all terms of service agreements of a person by id
    /// </summary>
    Task<List<PersonTermsOfServiceAgreement>> GetAllTermsOfServiceAgreementsByPersonId(Guid personId);

    /// <summary>
    /// Get newest terms of service agreement by person id
    /// </summary>
    Task<PersonTermsOfServiceAgreement?> GetNewestTermsOfServiceAgreementByPersonId(Guid personId);

    /// <summary>
    /// Add new terms of service agreement
    /// </summary>
    Task<PersonTermsOfServiceAgreement> AddNewTermsOfServiceAgreement(TermsOfService termsOfService, Guid personId);

    /// <summary>
    /// Remove terms of service agreement
    /// </summary>
    Task RemoveTermsOfServiceAgreement(PersonTermsOfServiceAgreement termsOfServiceAgreement);
}

