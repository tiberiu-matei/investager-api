using System.Collections.Generic;

namespace Investager.Infrastructure.Models;

public class LokiStream
{
    public IDictionary<string, string> Stream { get; set; }

    public IEnumerable<IEnumerable<string>> Values { get; set; }
}
