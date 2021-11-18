using System.Collections.Generic;

namespace Investager.Core.Dtos;

public class TimeSeriesSummary
{
    public string Key { get; set; }

    public ICollection<TimePointResponse> Points { get; set; }

    public GainLossResponse GainLoss { get; set; }
}
