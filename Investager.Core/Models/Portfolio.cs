using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Portfolio
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public ICollection<PortfolioAsset> PortfolioAssets { get; set; } = new List<PortfolioAsset>();
    }
}
