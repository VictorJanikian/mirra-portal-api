using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Database;
using Mirra_Portal_API.Database.Repositories;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services;
using Mirra_Portal_API.Services.Interfaces;

IConfiguration configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.UseMemberCasing());

standardizeErrorResponses(builder.Services);
addDatabaseContext(builder.Services);
addAutoMapper(builder.Services);
addServices(builder.Services);

var app = builder.Build();

app.UseCors("AllowAllPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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
    services.AddScoped<ICustomerRepository, CustomerRepository>();
}

