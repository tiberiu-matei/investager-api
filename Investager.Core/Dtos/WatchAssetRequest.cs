namespace Investager.Core.Dtos
{
    public class WatchAssetRequest
    {
        public int UserId { get; set; }

        public int WatchlistId { get; set; }

        public int AssetId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
