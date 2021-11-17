namespace Investager.Core.Models
{
    public class WatchlistAsset
    {
        public int WatchlistId { get; set; }

        public Watchlist Watchlist { get; set; }

        public int AssetId { get; set; }

        public Asset Asset { get; set; }

        public int DisplayOrder { get; set; }
    }
}
