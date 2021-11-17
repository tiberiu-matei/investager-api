using Investager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAll();

        Task<IEnumerable<CurrencyPair>> GetPairs();

        Task Add(Currency currency);

        Task AddPair(CurrencyPair currencyPair);
    }
}
