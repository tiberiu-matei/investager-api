using System.Collections.Generic;

namespace Investager.Core.Dtos
{
    public class PortfolioDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<int> AssetIds { get; set; }
    }
}
