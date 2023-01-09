using System.Reflection;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Middleware;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;
using VirtualFinland.UserAPI.Helpers.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient("", _ => { });

// Validate server configurations
ServerConfigurationValidation.ValidateServer(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var securityScheme = new OpenApiSecurityScheme
{
    Name = HeaderNames.Authorization,
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security"
};

var securityReq = new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = SecuritySchemeType.Http.ToString(),
            In = ParameterLocation.Header
        },
        Array.Empty<string>()
    }
};

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(config =>
{
    //use fully qualified object names
    config.CustomSchemaIds(s => s.FullName?.Replace("+", "."));
    config.EnableAnnotations();
    config.AddSecurityDefinition("Bearer", securityScheme);
    config.AddSecurityRequirement(securityReq);
    config.SchemaFilter<SwaggerSkipPropertyFilter>();
});

AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();

var secret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
    ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
    : null;
var dbConnectionString = secret ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString,
        op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>()));
});

IIdentityProviderConfig testBedIdentityProviderConfig = new TestBedIdentityProviderConfig(builder.Configuration);
testBedIdentityProviderConfig.LoadOpenIdConfigUrl();

IIdentityProviderConfig sinunaIdentityProviderConfig = new SinunaIdentityProviderConfig(builder.Configuration);
sinunaIdentityProviderConfig.LoadOpenIdConfigUrl();

builder.Services.AddAuthentication()
    .AddJwtBearer(Constants.Security.TestBedBearerScheme, c =>
    {
        JwksExtension.SetJwksOptions(c, new JwkOptions(testBedIdentityProviderConfig.JwksOptionsUrl));

        c.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = testBedIdentityProviderConfig.Issuer
        };
    }).AddJwtBearer(Constants.Security.SuomiFiBearerScheme, c =>
    {
        JwksExtension.SetJwksOptions(c, new JwkOptions(builder.Configuration["AuthGW:JwksJsonURL"]));
        c.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AuthGW:Issuer"]
        };
    })
    .AddJwtBearer(Constants.Security.SinunaScheme, c =>
    {
        JwksExtension.SetJwksOptions(c, new JwkOptions(sinunaIdentityProviderConfig.JwksOptionsUrl));

        c.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = sinunaIdentityProviderConfig.Issuer
        };
    });

builder.Services.AddAuthorization(options =>
{
    var allAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
        .AddAuthenticationSchemes(
            Constants.Security.TestBedBearerScheme,
            Constants.Security.SuomiFiBearerScheme,
            Constants.Security.SinunaScheme
        ).Build();

    options.AddPolicy(Constants.Security.AllPoliciesPolicy, allAuthorizationPolicyBuilder);
    options.DefaultPolicy = allAuthorizationPolicyBuilder;
});

builder.Services.AddResponseCaching();

builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<IOccupationsFlatRepository, OccupationsFlatRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();
builder.Services.AddTransient<UserSecurityService>();
builder.Services.AddTransient<AuthenticationService>();
builder.Services.AddTransient<AuthGwVerificationService>();
builder.Services.AddFluentValidation(new[] {Assembly.GetExecutingAssembly()});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (EnvironmentExtensions.IsDevelopment(app.Environment) || EnvironmentExtensions.IsStaging(app.Environment))
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCaching();

// Pre-Initializations and server start optimizations
using (var scope = app.Services.CreateScope())
{
    // Initialize automatically any database changes
    var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dataContext.Database.MigrateAsync();

    var occupationsRepository = scope.ServiceProvider.GetRequiredService<IOccupationsRepository>();
    var occupationsFlatRepository = scope.ServiceProvider.GetRequiredService<IOccupationsFlatRepository>();
    var languageRepository = scope.ServiceProvider.GetRequiredService<ILanguageRepository>();
    var countriesRepository = scope.ServiceProvider.GetRequiredService<ICountriesRepository>();
    
    Task.WaitAll(
        occupationsRepository.GetAllOccupations(), 
        occupationsFlatRepository.GetAllOccupationsFlat(), 
        languageRepository.GetAllLanguages(), 
        countriesRepository.GetAllCountries()
    );

    // Warmup Entity Framework ORM by calling the related features to desired HTTP requests
    var mediator = scope.ServiceProvider.GetService<IMediator>();
    var updateUserWarmUpCommand = new UpdateUser.Command(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
    updateUserWarmUpCommand.SetAuth(UsersDbContext.WarmUpUserId);
    
    await mediator?.Send(new GetUser.Query(UsersDbContext.WarmUpUserId))!;
    await mediator?.Send(updateUserWarmUpCommand)!;
    await mediator?.Send(new VerifyIdentityUser.Query(string.Empty, string.Empty))!;

}


app.MapGet("/", () => "App is up!");

app.Run();
