using System.Reflection;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Middleware;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Security.Extensions;
using VirtualFinland.UserAPI.Helpers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.Extensions.Options;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Bootsrapping environment: {builder.Environment.EnvironmentName}");

//
// App runtime configuration
//
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient("", _ => { });
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});

// @see: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-6.0
builder.Services.AddDataProtection()
    .SetApplicationName("VirtualFinland.UsersAPI")
    .PersistKeysToDbContext<UsersDbContext>();
builder.Services.AddDataProtection().UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

//
// Swagger setup
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    //use fully qualified object names
    config.CustomSchemaIds(s => s.FullName?.Replace("+", "."));
    config.EnableAnnotations();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JSON Web Token based security"
    });
    config.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
    config.SchemaFilter<SwaggerSkipPropertyFilter>();
});

//
// Database connection
//
AwsConfigurationManager awsConfigurationManager = new();

var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
    ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
    : null;
var dbConnectionString = databaseSecret ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString,
        op => op
            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>())
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
        );
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // @TODO: Resolve what changed in datetime inserting that causes this to be needed

//
// Redis connection
//
var redisEndpoint = Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? builder.Configuration["Redis:Endpoint"];
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisEndpoint},abortConnect=false,connectRetry=5");

//
// App security
//
builder.Services.RegisterSecurityFeatures(builder.Configuration, redis);
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHanderMiddleware>();
builder.Services.AddTransient<AuthenticationService>();
builder.Services.RegisterConsentServiceProviders(builder.Configuration);
builder.Services.AddTransient<TestbedConsentSecurityService>();

//
// Route handlers
//
builder.Services.AddControllers();
builder.Services.AddTransient<ProblemDetailsFactory, ValidationProblemDetailsFactory>();
builder.Services.AddFluentValidation(new[] { Assembly.GetExecutingAssembly() });
builder.Services.AddResponseCaching();

//
// Data repositories
//
builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<IOccupationsFlatRepository, OccupationsFlatRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();
builder.Services.AddSingleton<ITermsOfServiceRepository, TermsOfServiceRepository>();

//
// Other dependencies
//
builder.Services.AddSingleton<CodesetConfig>();
builder.Services.AddSingleton<CodesetsService>();

//
// Application build
//
var app = builder.Build();

// Use swagger only in non-production environments
if (!EnvironmentExtensions.IsProduction(app.Environment))
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}


app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items["Exception"] is Exception exception)
        {
            // Add the exception to the log context, omit the stack trace
            diagnosticContext.Set("@x", $"{exception.GetType().Name}: {exception.Message}");
        }
    };
});
app.UseMiddleware<RequestTracingMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCaching();

app.MapGet("/", () => "App is up!");

app.Run();
