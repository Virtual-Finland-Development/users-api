# API Security Features

Users API provides resources that are intented to be used by other applications using an external authentication provider. Some routes are accessed directly by a specific application and other are intented to be accessed through a dataspace gateway service. Access is also restricted to only authenticated requests. For these use-cases there are different security mechanisms that restrict the access only to the authorized applications. 

## Request Authorization

Users API supports three different authentication providers: 
- [Sinuna](https://sinuna.fi/)
- [Testbed](https://testbed.fi/)
- [Virtual Finland Login](https://accessfinland.com)

The different authentication providers are configured (for example disabled or enabled) in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file properties block: `Security:Authorization`. The configurations also include the authentication provider specific settings like OpenID Connect endpoints. 

By default both authentication providers are disabled and are intented to be enabled by overriding the configuration with the stage specific configuration file (for example [appsettings.mvp-staging.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.mvp-staging.json)) and/or by using the setting override environment variables (for example `Security__Authorization__Sinuna__IsEnabled`) in the deployment phase.

The request authorization is enforced at the Controller level using the `Authorize` attribute. The attribute is configured with a policies that are set up in the [SecurityFeatureServiceExtensions.cs](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Security/Extensions/SecurityFeatureServiceExtensions.cs)-file. During the authorization phase the authentication token is validated and the claims are extracted from the token. The claims are then used to validate the request against the configured policies. The specific authentication provider policy is selected using the authentication token issuer claim, and is accepted only if the issuer/provider is configured and enabled.

### Audience Validation

Users API supports a couple of different approaches on validating the audience claim in the authentication token. The audience claim is used to validate that the token is intented to be used by the API. Users API treats the audience claim as an identity of the external application and with that it's possible to restrict access only to specific applications. The supported approaches are using a static configuration value list of allowed audiences or using external service API to validate the audience exists and belongs in an allowed group.

The audience validation is configured in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file properties block: `Security:Authorization:<provider>:AudienceGuard` and has settings for both approaches: `StaticConfig` and `Service`. The Service -approach requires the security feature (authentication provider implementation) implement the `ValidateSecurityTokenAudienceByService()` method of the [SecurityFeature](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Security/Features/SecurityFeature.cs) class. 

Currently only Sinuna authentication provider has the implementation, as defined in the [SinunaSecurityFeature.cs](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Security/Features/SinunaSecurityFeature.cs)-file. With Sinuna, the audience guard service ([DataspaceAudienceSecurityService.cs](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Helpers/Services/DataspaceAudienceSecurityService.cs)-file) retrieves the audience from the dataspace service API and validates that the audience is configured in a group of intrest. The allowed groups are configured in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file properties block: `Security:Authorization:Sinuna:AudienceGuard:Service:AllowedGroups`.

### The enforcement of Access Finland MVP Terms of Service Agreement

When used in the context of [Access Finland MVP](https://github.com/Virtual-Finland-Development/access-finland) application the users-api request authentication phase requires that the user has agreed to the terms of service of the Access Finland MVP. The enforcement can be enabled or disabled using with the `Security.Options.TermsOfServiceAgreementRequired` property in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file and is disabled by default. The actual enforcement is implemented in the `VerifyPersonTermsOfServiceAgreement` method of the [ApplicationSecurity.cs](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Security/ApplicationSecurity.cs)-file.

Read more at [./README.terms-of-service.md](./README.terms-of-service.md) document.

## Controller Specific Guards

### RequestFromAccessFinland Policy

The [UserController](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Activities/User/UserController.cs) is a collection of routes that are intented to be used directly by the Access Finland application. The routes are protected with the `Authorize(Policy = "RequestFromAccessFinland")` attribute that requires the request to be authenticated with a custom header `X-Api-Key` that is a shared secret between the apps and is configured in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file properties block: `Security:Access:AccessFinland`.

### RequestFromDataspace Policy

The [ProductizerController](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/Activities/Productizer/ProductizerController.cs) defines the routes that are to be used to handle the requests that come from the dataspace gateway service. The routes are protected with the `Authorize(Policy = "RequestFromDataspace")` attribute that requires that the requests `User-Agent` header matches the one configured in the [appsettings.json](../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/appsettings.json)-file properties block: `Security:Access:Dataspace`.

## Endpoint specific guards

### Allow anonymous

Endpoints that are intented to be used by the public are flagged with the `AllowAnonymous` attribute. Read more here: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-6.0

The only users-api endpoint that is intented to be used by the public is the `GET /health-check` endpoint that is used to check the health of the API. 
