using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class CurrencyExchange
    {
        public int Id { get; set; }

        public string FirstCurrencyId { get; set; } = default!;

        public string FirstCurrencyName { get; set; } = default!;

        public string FirstCurrencyType { get; set; } = default!;

        public string SecondCurrency { get; set; } = default!;

        public bool HasData { get; set; }

        public ICollection<Portfolio> Portfolios = new List<Portfolio>();
    }
}
