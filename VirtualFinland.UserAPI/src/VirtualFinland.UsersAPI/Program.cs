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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

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

// Validate server configuration
ServerConfigurationValidation.ValidateServer(builder.Configuration);

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
AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();

var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
    ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
    : null;
var dbConnectionString = databaseSecret ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton<IAuditInterceptor, AuditInterceptor>();
builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString,
        op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>()));
});

//
// App security
//
builder.Services.RegisterSecurityFeatures(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHanderMiddleware>();
builder.Services.AddTransient<UserSecurityService>();
builder.Services.AddTransient<AuthenticationService>();
builder.Services.RegisterConsentServiceProviders(builder.Configuration);
builder.Services.AddTransient<TestbedConsentSecurityService>();

//
// Route handlers
//
builder.Services.AddControllers();
builder.Services.AddFluentValidation(new[] { Assembly.GetExecutingAssembly() });
builder.Services.AddResponseCaching();

//
// Data repositories
//
builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<IOccupationsFlatRepository, OccupationsFlatRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();

//
// Other dependencies
//
builder.Services.Configure<CodesetConfig>(builder.Configuration);

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


app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCaching();

// Only run database migrations in local environment
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
