using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mirra_Portal_API.Database;
using Mirra_Portal_API.Database.Repositories;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Integration;
using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Middleware.Identity;
using Mirra_Portal_API.Middleware.Logging;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Security;
using Mirra_Portal_API.Services;
using Mirra_Portal_API.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.UseMemberCasing());

standardizeErrorResponses(builder.Services);
addDatabaseContext(builder.Services);
addAutoMapper(builder.Services);
addServices(builder.Services);
configureJwt(builder.Services);

var app = builder.Build();

app.UseCors("AllowAllPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<IdentityMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.Run();


void standardizeErrorResponses(IServiceCollection services)
{
    services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();
            var errorMessage = string.Join(" / ", errors);
            var errorResponse = new ErrorResponse(errorMessage);
            return new BadRequestObjectResult(errorResponse);
        };
    });
}
static void addDatabaseContext(IServiceCollection services)
{
    services.AddDbContext<DatabaseContext>();
}

void addAutoMapper(IServiceCollection services)
{
    services.AddAutoMapper(config =>
    {
        config.AllowNullCollections = true;
        config.AllowNullDestinationValues = true;
        config.CreateMap<int?, int>()
            .ConvertUsing(
                    (int? src, int dest, ResolutionContext _) => src ?? dest
            );
    },
        AppDomain.CurrentDomain.GetAssemblies());
}

void addServices(IServiceCollection services)
{
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<ILoginService, LoginService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IConfigurationService, ConfigurationService>();
    services.AddScoped<IEmailIntegration, EmailIntegration>();
    services.AddScoped<ICustomerContentPlatformConfigurationRepository, CustomerContentPlatformConfigurationRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<ISchedulingRepository, SchedulingRepository>();
    services.AddScoped<IdentityHelper>();
    services.AddScoped<SymmetricEncryptionHelper>();
}

void configureJwt(IServiceCollection services)
{
    var jwtPrivateKey = configuration.GetValue<string>("jwtprivatekey");
    var signingConfigurations = new SigningConfigurations(jwtPrivateKey);
    services.AddSingleton(signingConfigurations);

    var tokenConfigurations = new TokenConfigurations();
    new ConfigureFromConfigurationOptions<TokenConfigurations>(
        configuration.GetSection("TokenConfigurations"))
            .Configure(tokenConfigurations);
    services.AddSingleton(tokenConfigurations);

    var jwtPublicKey = configuration.GetValue<string>("jwtpublickey");
    services.AddJwtSecurity(tokenConfigurations, jwtPublicKey);
}
