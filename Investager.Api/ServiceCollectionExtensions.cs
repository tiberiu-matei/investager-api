using Investager.Api.HostedServices;
using Investager.Api.Policies;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Helpers;
using Investager.Infrastructure.Persistence;
using Investager.Infrastructure.Services;
using Investager.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Investager.Api;

public static class ServiceCollectionExtensions
{
    public static void AddInvestagerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAssetDataService, AlpacaService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAuthorizationHandler, AuthenticatedUserHandler>();
        services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
        services.AddScoped<ICurrencyPairDataService, CoinGeckoService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ITimeSeriesRepository, TimeSeriesRepository>();
        services.AddScoped<ITimeSeriesService, TimeSeriesService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWatchlistService, WatchlistService>();

        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IFuzzyMatch, LevenshteinFuzzyMatch>();
        services.AddTransient<IPasswordHelper, PasswordHelper>();
        services.AddTransient<ITimeHelper, TimeHelper>();

        services.AddSingleton<ICache, Cache>();
        services.AddSingleton(new AlpacaSettings());
        services.AddSingleton(new CoinGeckoSettings());
        services.AddSingleton(new DataScanSettings());
        services.AddSingleton(new DataUpdateSettings());
        services.AddSingleton(new LokiSettings());

        services.AddHostedService<AssetDataUpdateService>();
        services.AddHostedService<AssetScanService>();
        services.AddHostedService<CurrencyPairDataUpdateService>();
        services.AddHostedService<CurrencyPairScanService>();

        var secretKey = configuration["JwtSecretKey"];
        var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        services.AddSingleton(signingCredentials);
    }

    public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(HttpClients.AlpacaPaper, e =>
        {
            e.BaseAddress = new Uri("https://paper-api.alpaca.markets/");
            e.DefaultRequestHeaders.Add("APCA-API-KEY-ID", configuration.GetSection("Alpaca")["KeyId"]);
            e.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", configuration.GetSection("Alpaca")["SecretKey"]);
        }).AddPolicyHandler(PollyPolicies.GetRetryPolicy());

        services.AddHttpClient(HttpClients.AlpacaData, e =>
        {
            e.BaseAddress = new Uri("https://data.alpaca.markets/");
            e.DefaultRequestHeaders.Add("APCA-API-KEY-ID", configuration.GetSection("Alpaca")["KeyId"]);
            e.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", configuration.GetSection("Alpaca")["SecretKey"]);
        }).AddPolicyHandler(PollyPolicies.GetRetryPolicy());

        services.AddHttpClient(HttpClients.CoinGecko, e =>
        {
            e.BaseAddress = new Uri("https://api.coingecko.com/api/");
        }).AddPolicyHandler(PollyPolicies.GetRetryPolicy());
    }
}
