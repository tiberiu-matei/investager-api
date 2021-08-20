namespace Investager.Core.Dtos
{
    public class StarredAssetResponse
    {
        public int AssetId { get; set; }

        public string Symbol { get; set; }

        public string Exchange { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public string Industry { get; set; }

        public string Currency { get; set; }

        public int DisplayOrder { get; set; }

        public GainLossResponse GainLoss { get; set; }
    }
}
