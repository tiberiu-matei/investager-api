using System;
using System.Text.Json.Serialization;

namespace Investager.Infrastructure.Models
{
    public class AlpacaAssetPrice
    {
        [JsonPropertyName("t")]
        public DateTime Time { get; set; }

        [JsonPropertyName("o")]
        public float Open { get; set; }

        [JsonPropertyName("h")]
        public float High { get; set; }

        [JsonPropertyName("l")]
        public float Low { get; set; }

        [JsonPropertyName("c")]
        public float Close { get; set; }

        [JsonPropertyName("v")]
        public ulong Volume { get; set; }
    }
}
