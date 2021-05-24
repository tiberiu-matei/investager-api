using System.Collections.Generic;

namespace Investager.Core.Dtos
{
    public class UpdatePortfolioDto
    {
        public string Name { get; set; }

        public IEnumerable<int> AssetIds { get; set; }
    }
}
