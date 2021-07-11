using System.Collections.Generic;

namespace Investager.Infrastructure.Models
{
    public class LokiRequest
    {
        public IEnumerable<LokiStream> Streams { get; set; }
    }
}
