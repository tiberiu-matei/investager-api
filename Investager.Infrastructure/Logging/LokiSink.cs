using Investager.Core.Models;
using Investager.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Logging
{
    public class LokiSink : ILogEventSink, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILokiFormatter _lokiFormatter;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LokiSettings _lokiSettings;
        private readonly object _logsLock = new object();
        private readonly long _unixEpochTicks;
        private readonly List<LogEvent> _logs = new List<LogEvent>();

        private CancellationTokenSource _cancellationTokenSource;
        private Dictionary<string, string> _lokiStream;

        public LokiSink(
            IConfiguration configuration,
            ILokiFormatter lokiFormatter,
            IHttpClientFactory httpClientFactory,
            LokiSettings lokiSettings)
        {
            _configuration = configuration;
            _lokiFormatter = lokiFormatter;
            _httpClientFactory = httpClientFactory;
            _lokiSettings = lokiSettings;

            _unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

            Start();
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_logsLock)
            {
                _logs.Add(logEvent);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            SendLogs().GetAwaiter().GetResult();
        }

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                _lokiStream = new Dictionary<string, string>
                {
                    { "host", Dns.GetHostName() },
                    { "env", _configuration[ConfigKeys.Environment] }
                };

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(_lokiSettings.BatchInterval);

                    await SendLogs();
                }
            }, _cancellationTokenSource.Token);
        }

        private async Task SendLogs()
        {
            var batches = new List<List<LogEvent>>();
            lock (_logsLock)
            {
                if (_logs.Any())
                {
                    for (var i = 0; i < _logs.Count; i += _lokiSettings.MaxBatchSize)
                    {
                        batches.Add(_logs.GetRange(i, Math.Min(_lokiSettings.MaxBatchSize, _logs.Count - i)));
                    }

                    _logs.Clear();
                }
            }

            foreach (var batch in batches)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient(HttpClients.Loki);

                    var request = new LokiRequest
                    {
                        Streams = new List<LokiStream>
                        {
                            new LokiStream
                            {
                                Stream = _lokiStream,
                                Values = MapLogs(batch),
                            }
                        }
                    };

                    using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    content.Headers.ContentType.CharSet = "";

                    var response = await client.PostAsync("loki/api/v1/push", content);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    using var lokiErrorLogger = new LoggerConfiguration()
                        .WriteTo.File("logs/loki-errors.log", rollingInterval: RollingInterval.Month)
                        .CreateLogger();

                    lokiErrorLogger.Error(ex, "Error sending {@Batch} to Loki.", batch);
                }
            }
        }

        private IList<string[]> MapLogs(IList<LogEvent> logs)
        {
            return logs.Select(log =>
            {
                var unixEpochNs = ((log.Timestamp.UtcTicks - _unixEpochTicks) * 100).ToString();

                var logLine = _lokiFormatter.Format(log);

                return new string[2] { unixEpochNs, logLine };
            }).ToList();
        }
    }
}
