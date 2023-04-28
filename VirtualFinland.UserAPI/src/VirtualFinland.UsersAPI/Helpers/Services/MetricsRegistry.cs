using Prometheus;

namespace VirtualFinland.UserAPI.Helpers.Services;

public static class MetricsRegistry
{
    public static Counter AuthenticationRequestsCounter => Metrics.CreateCounter(
        "users_api_authentication_request_counter",
        "Number of authentication request received by API", "authentication", "anotherOnce");

    public static Counter UsersCreated =>
        Metrics.CreateCounter("user_api_user_created", "Number of users created", "user_management");

    public static Gauge JobApplicantProfileReadDuration =>
        Metrics.CreateGauge("productizer_job_applicant_profile_read_duration", "number of something");
}
