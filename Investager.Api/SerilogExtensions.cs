﻿using Investager.Core.Models;
using Investager.Infrastructure.Helpers;
using Investager.Infrastructure.Logging;
using Investager.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting.Display;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Investager.Api
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration Loki(
            this LoggerSinkConfiguration loggerConfiguration,
            IConfiguration configuration,
            LokiSettings lokiSettings)
        {
            var lokiServices = new ServiceCollection();

            lokiServices.AddHttpClient(HttpClients.Loki, e =>
            {
                var section = "Loki";
                e.BaseAddress = new Uri(configuration.GetSection(section)["BaseUrl"]);

                var username = configuration.GetSection(section)["Username"];
                var password = configuration.GetSection(section)["Password"];
                var authBytes = Encoding.ASCII.GetBytes($"{username}:{password}");

                e.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
            }).AddPolicyHandler(PollyPolicies.GetRetryPolicy());

            var provider = lokiServices.BuildServiceProvider();

            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            var template = "Level={Level} {Message}{NewLine}{Exception}";
            var formatter = new MessageTemplateTextFormatter(template, CultureInfo.InvariantCulture);

            return loggerConfiguration.Sink(new LokiSink(configuration, formatter, httpClientFactory, lokiSettings));
        }
    }
}
