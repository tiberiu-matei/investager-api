using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Factories;
using Investager.Infrastructure.Helpers;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Persistence;
using Investager.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Investager.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInvestagerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
            services.AddScoped<ITimeSeriesPointRepository, TimeSeriesPointRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IPasswordHelper, PasswordHelper>();
            services.AddTransient<ITimeHelper, TimeHelper>();
            services.AddTransient<IDataProviderServiceFactory, DataProviderServiceFactory>();
            services.AddTransient<IJwtTokenService, JwtTokenService>();

            services.AddSingleton<IDataCollectionServiceFactory, DataCollectionServiceFactory>();
            services.AddSingleton(new AlpacaSettings());

            var secretKey = configuration["JwtSecretKey"];
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            services.AddSingleton(signingCredentials);
        }
    }
}
