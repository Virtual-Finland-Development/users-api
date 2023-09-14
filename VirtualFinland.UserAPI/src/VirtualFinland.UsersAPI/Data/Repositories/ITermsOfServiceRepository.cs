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
    Task<List<PersonTermsOfServiceAgreement>> GetAllTermsOfServiceAgreementsByPersonId(Guid personId, string audience);

    /// <summary>
    /// Get the latest terms of service agreement by person id
    /// </summary>
    Task<PersonTermsOfServiceAgreement?> GetTheLatestTermsOfServiceAgreementByPersonId(Guid personId, string audience);

    /// <summary>
    /// Get the agreement of the latest terms of service by person id
    /// </summary>
    Task<PersonTermsOfServiceAgreement?> GetTermsOfServiceAgreementOfTheLatestTermsByPersonId(Guid personId, string audience);

    /// <summary>
    /// Add new terms of service agreement
    /// </summary>
    Task<PersonTermsOfServiceAgreement> AddNewTermsOfServiceAgreement(TermsOfService termsOfService, Guid personId, string audience);

    /// <summary>
    /// Remove terms of service agreement
    /// </summary>
    Task RemoveTermsOfServiceAgreement(PersonTermsOfServiceAgreement termsOfServiceAgreement);
}

