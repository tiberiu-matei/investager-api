namespace Investager.Core.Models
{
    public class Currency
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public CurrencyType Type { get; set; }

        public string ProviderId { get; set; }
    }
}
