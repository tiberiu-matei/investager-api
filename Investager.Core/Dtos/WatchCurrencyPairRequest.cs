namespace Investager.Core.Dtos
{
    public class WatchCurrencyPairRequest
    {
        public int UserId { get; set; }

        public int WatchlistId { get; set; }

        public int CurrencyPairId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
