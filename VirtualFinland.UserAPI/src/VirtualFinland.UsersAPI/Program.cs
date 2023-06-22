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
using VirtualFinland.UserAPI.Security;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

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

//
// Swagger
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
// Database
//
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

//
// App security
//
var securityBuilder = new ApplicationSecurity(builder.Configuration);
securityBuilder.BuildSecurity(builder);
builder.Services.AddSingleton<IApplicationSecurity>(securityBuilder);
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHanderMiddleware>();
builder.Services.AddSingleton<IConsentProviderConfig>(securityBuilder.testBedConsentProviderConfig);
builder.Services.AddTransient<TestbedConsentSecurityService>();
builder.Services.AddTransient<UserSecurityService>();
builder.Services.AddTransient<AuthenticationService>();

//
// Data repositories, dependencies and route handlers
//
builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<IOccupationsFlatRepository, OccupationsFlatRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();
builder.Services.AddFluentValidation(new[] { Assembly.GetExecutingAssembly() });
builder.Services.Configure<CodesetConfig>(builder.Configuration);
builder.Services.AddResponseCaching();

//
// App buildup
//
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
