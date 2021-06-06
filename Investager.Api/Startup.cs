using Investager.Api.Middleware;
using Investager.Api.Policies;
using Investager.Core.Mapping;
using Investager.Core.Models;
using Investager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace Investager.Api
{
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

            services.AddHttpClient(HttpClients.AlpacaPaper, e =>
            {
                e.BaseAddress = new Uri("https://paper-api.alpaca.markets/");
                e.DefaultRequestHeaders.Add("APCA-API-KEY-ID", Configuration.GetSection("Alpaca")["KeyId"]);
                e.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", Configuration.GetSection("Alpaca")["SecretKey"]);
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient(HttpClients.AlpacaData, e =>
            {
                e.BaseAddress = new Uri("https://data.alpaca.markets/");
                e.DefaultRequestHeaders.Add("APCA-API-KEY-ID", Configuration.GetSection("Alpaca")["KeyId"]);
                e.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", Configuration.GetSection("Alpaca")["SecretKey"]);
            }).AddPolicyHandler(GetRetryPolicy());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(e => e.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
