using System.Reflection;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NetDevPack.Security.JwtExtensions;
using Serilog;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Middleware;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;
using VirtualFinland.UserAPI.Helpers.Extensions;
using System.IdentityModel.Tokens.Jwt;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
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

var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
    ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
    : null;
var dbConnectionString = databaseSecret ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString,
        op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>()));
});

IIdentityProviderConfig testBedIdentityProviderConfig = new TestBedIdentityProviderConfig(builder.Configuration);
testBedIdentityProviderConfig.LoadOpenIdConfigUrl();

IConsentProviderConfig testBedConsentProviderConfig = new TestBedConsentProviderConfig(builder.Configuration);
testBedConsentProviderConfig.LoadPublicKeys();

IIdentityProviderConfig sinunaIdentityProviderConfig = new SinunaIdentityProviderConfig(builder.Configuration);
sinunaIdentityProviderConfig.LoadOpenIdConfigUrl();


// @see: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
    options.DefaultChallengeScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
})
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
        c.RequireHttpsMetadata = !EnvironmentExtensions.IsLocal(builder.Environment);
        JwksExtension.SetJwksOptions(c, new JwkOptions(builder.Configuration["SuomiFi:AuthorizationJwksJsonUrl"]));
        c.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["SuomiFi:Issuer"]
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
    }).AddPolicyScheme(Constants.Security.ResolvePolicyFromTokenIssuer, Constants.Security.ResolvePolicyFromTokenIssuer, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            {
                var token = authorization.Substring("Bearer ".Length).Trim();
                var jwtHandler = new JwtSecurityTokenHandler();

                if (jwtHandler.CanReadToken(token))
                {
                    var issuer = jwtHandler.ReadJwtToken(token).Issuer;
                    switch (issuer)
                    {
                        // Cheers: https://stackoverflow.com/a/65642709
                        case var value when value == testBedIdentityProviderConfig.Issuer:
                            return Constants.Security.TestBedBearerScheme;
                        case var value when value == sinunaIdentityProviderConfig.Issuer:
                            return Constants.Security.SinunaScheme;
                        case var value when value == builder.Configuration["SuomiFi:Issuer"]:
                            return Constants.Security.SuomiFiBearerScheme;
                    }
                }
            }
            return Constants.Security.TestBedBearerScheme; // Defaults to testbed
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Security.TestBedBearerScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(Constants.Security.TestBedBearerScheme);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy(Constants.Security.SuomiFiBearerScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(Constants.Security.SuomiFiBearerScheme);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy(Constants.Security.SinunaScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(Constants.Security.SinunaScheme);
        policy.RequireAuthenticatedUser();
    });
});


builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHanderMiddleware>();

builder.Services.AddResponseCaching();

builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<IOccupationsFlatRepository, OccupationsFlatRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();
builder.Services.AddSingleton<IConsentProviderConfig>(testBedConsentProviderConfig);
builder.Services.AddTransient<TestbedConsentSecurityService>();
builder.Services.AddTransient<UserSecurityService>();
builder.Services.AddTransient<AuthenticationService>();
builder.Services.AddFluentValidation(new[] { Assembly.GetExecutingAssembly() });
builder.Services.Configure<CodesetConfig>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCaching();

if (EnvironmentExtensions.IsLocal(app.Environment))
{
    using (var scope = app.Services.CreateScope())
    {
        Log.Information("Migrate database");

        // Initialize automatically any database changes
        var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await dataContext.Database.MigrateAsync();

        Log.Information("Database migration completed");
    }
}


app.MapGet("/", () => "App is up!");

app.Run();
