using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Watchlist
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public ICollection<WatchlistAsset> Assets { get; set; }

        public ICollection<WatchlistCurrencyPair> CurrencyPairs { get; set; }
    }
}
