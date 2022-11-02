using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var securityScheme = new OpenApiSecurityScheme()
{
    Name = HeaderNames.Authorization,
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
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
        new string[]
        {
        }
    }
};

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(config =>
{ //use fully qualified object names
  config.CustomSchemaIds(s => s.FullName?.Replace("+", "."));
  config.EnableAnnotations();
  config.AddSecurityDefinition("Bearer", securityScheme);
  config.AddSecurityRequirement(securityReq);
  config.SchemaFilter<SwaggerSkipPropertyFilter>();
});

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsersDbContext>(options => { options.UseNpgsql(dbConnectionString, op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>())); });

IIdentityProviderConfig testBedIdentityProviderConfig = new TestBedIdentityProviderConfig(builder.Configuration);
testBedIdentityProviderConfig.LoadOpenIdConfigUrl();

IIdentityProviderConfig sinunaIdentityProviderConfig = new SinunaIdentityProviderConfig(builder.Configuration);
sinunaIdentityProviderConfig.LoadOpenIdConfigUrl();

builder.Services.AddAuthentication("TestBedScheme")
    .AddJwtBearer("TestBedScheme", c =>
    { 
      c.SetJwksOptions(new JwkOptions(testBedIdentityProviderConfig.JwksOptionsUrl));

      c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateActor = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = testBedIdentityProviderConfig.Issuer
      }; }).AddJwtBearer("SinunaScheme", c =>
    { 
    c.SetJwksOptions(new JwkOptions(sinunaIdentityProviderConfig.JwksOptionsUrl));

    c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateActor = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = sinunaIdentityProviderConfig.Issuer
    }; });

builder.Services.AddAuthorization();
builder.Services.AddResponseCaching();

builder.Services.AddSingleton<IOccupationsRepository, OccupationsRepository>();
builder.Services.AddSingleton<ILanguageRepository, LanguageRepository>();
builder.Services.AddSingleton<ICountriesRepository, CountriesRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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
// Notice: Keep the IdentityProviderAuthMiddleware between the authentication and authorizations middlewares.
app.UseIdentityProviderAuthMiddleware();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCaching();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dataContext.Database.MigrateAsync();
}


app.MapGet("/", () => "App is up!");

app.Run();