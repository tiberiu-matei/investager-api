using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class CurrencyExchange
    {
        public int Id { get; set; }

        public string FirstCurrencyId { get; set; }

        public string FirstCurrencyName { get; set; }

        public string FirstCurrencyType { get; set; }

        public string SecondCurrency { get; set; }

        public bool HasData { get; set; }

        public ICollection<Portfolio> Portfolios = new List<Portfolio>();
    }
}
