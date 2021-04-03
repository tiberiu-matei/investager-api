namespace Investager.Core.Models
{
    public class Stock
    {
        public uint Id { get; set; }

        public string Ticker { get; set; }

        public string Exchange { get; set; }

        public string Name { get; set; }

        public string Industry { get; set; }

        public string Currency { get; set; }
    }
}
