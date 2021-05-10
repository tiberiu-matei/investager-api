using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Factories;
using Investager.Infrastructure.Helpers;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Investager.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInvestagerServices(this IServiceCollection services)
        {
            services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
            services.AddScoped<ITimeSeriesPointRepository, TimeSeriesPointRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IPasswordHelper, PasswordHelper>();
            services.AddTransient<ITimeHelper, TimeHelper>();
            services.AddTransient<IDataProviderServiceFactory, DataProviderServiceFactory>();

            services.AddSingleton<IDataCollectionServiceFactory, DataCollectionServiceFactory>();
            services.AddSingleton(new AlpacaSettings());
        }
    }
}
