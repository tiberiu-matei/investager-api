using System;

namespace Investager.Core.Dtos
{
    public class UpdateCurrencyPairDataRequest
    {
        public string FirstCurrencyProviderId { get; set; }

        public string SecondCurrencyProviderId { get; set; }

        public string Key { get; set; }

        public DateTime? LatestPointTime { get; set; }
    }
}
