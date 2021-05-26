using System.Collections.Generic;

namespace Investager.Core.Dtos
{
    public class TimeSeriesResponse
    {
        public string Key { get; set; }

        public IEnumerable<TimePointResponse> Points { get; set; }
    }
}
