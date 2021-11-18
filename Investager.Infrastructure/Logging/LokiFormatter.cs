using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using System.IO;
using System.Linq;
using System.Text;

namespace Investager.Infrastructure.Logging;

public class LokiFormatter : ILokiFormatter
{
    private const int DefaultWriteBufferCapacity = 256;

    private readonly JsonValueFormatter _valueFormatter;

    public LokiFormatter()
    {
        _valueFormatter = new JsonValueFormatter(typeTagName: "$type");
    }

    public string Format(LogEvent logEvent)
    {
        var buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));

        buffer.Write("{\"@mt\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, buffer);

        var tokensWithFormat = logEvent.MessageTemplate.Tokens
            .OfType<PropertyToken>()
            .Where(pt => pt.Format != null);

        if (tokensWithFormat.Any())
        {
            buffer.Write(",\"@r\":[");
            var delim = "";
            foreach (var r in tokensWithFormat)
            {
                buffer.Write(delim);
                delim = ",";
                var space = new StringWriter();
                r.Render(logEvent.Properties, space);
                JsonValueFormatter.WriteQuotedJsonString(space.ToString(), buffer);
            }
            buffer.Write(']');
        }

        buffer.Write(",\"@l\":\"");
        buffer.Write(logEvent.Level);
        buffer.Write('\"');

        if (logEvent.Exception != null)
        {
            buffer.Write(",\"@x\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), buffer);
        }

        foreach (var property in logEvent.Properties)
        {
            var name = property.Key;
            if (name.Length > 0 && name[0] == '@')
            {
                name = '@' + name;
            }

            buffer.Write(',');
            JsonValueFormatter.WriteQuotedJsonString(name, buffer);
            buffer.Write(':');
            _valueFormatter.Format(property.Value, buffer);
        }

        buffer.Write('}');
        buffer.WriteLine();

        return buffer.ToString();
    }
}
