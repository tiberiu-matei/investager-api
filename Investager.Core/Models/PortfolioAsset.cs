namespace Investager.Core.Models
{
    public class PortfolioAsset
    {
        public int PortfolioId { get; set; }

        public Portfolio Portfolio { get; set; }

        public int AssetId { get; set; }

        public Asset Asset { get; set; }
    }
}
