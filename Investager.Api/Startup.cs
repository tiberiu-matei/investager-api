using Investager.Api.Middleware;
using Investager.Api.Policies;
using Investager.Core.Mapping;
using Investager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Text.Json.Serialization;

namespace Investager.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<InvestagerCoreContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("InvestagerCore")));

        services.AddDbContext<InvestagerTimeSeriesContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("InvestagerTimeSeries")),
            optionsLifetime: ServiceLifetime.Singleton);
        services.AddDbContextFactory<InvestagerTimeSeriesContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("InvestagerTimeSeries")));

        services.AddAutoMapper(e => e.AddProfile<AutoMapperProfile>());

        services.AddHttpContextAccessor();
        services.AddCors();
        services.AddControllers();
        services.AddInvestagerServices(Configuration);

        services.AddAuthorization(e =>
        {
            e.AddPolicy(PolicyNames.User, p => p.Requirements.Add(new AuthenticatedUserRequirement()));
        });

        services.AddHttpClients(Configuration);

        services.AddMvc()
            .AddJsonOptions(e =>
            {
                e.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();

        using var scope = app.ApplicationServices.CreateScope();
        var coreContext = scope.ServiceProvider.GetService<InvestagerCoreContext>();
        coreContext?.Database.Migrate();
        var timeSeriesContext = scope.ServiceProvider.GetService<InvestagerTimeSeriesContext>();
        timeSeriesContext?.Database.Migrate();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseRouting();

        app.UseCors(e => e
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin());

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/api/v1/serviceinfo", async context => await context.Response.WriteAsync("Hello from Investager API."));
            endpoints.MapControllers().RequireAuthorization(PolicyNames.User);
        });
    }
}
