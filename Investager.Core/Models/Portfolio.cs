using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Portfolio
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public User User { get; set; }

        public IEnumerable<Stock> Stocks { get; set; }
    }
}
