using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICoreUnitOfWork _unitOfWork;
    private readonly ICache _cache;

    public CurrencyService(ICoreUnitOfWork coreUnitOfWork, ICache cache)
    {
        _unitOfWork = coreUnitOfWork;
        _cache = cache;
    }

    public Task<IEnumerable<Currency>> GetAll()
    {
        return _cache.Get(CacheKeys.Currencies, () =>
        {
            return _unitOfWork
                .Currencies
                .GetAll();
        });
    }

    public Task<IEnumerable<CurrencyPair>> GetPairs()
    {
        return _cache.Get(CacheKeys.CurrencyPairs, () =>
        {
            return _unitOfWork
                .CurrencyPairs
                .GetAll(e => e.Include(x => x.FirstCurrency).Include(x => x.SecondCurrency));
        });
    }

    public async Task Add(Currency currency)
    {
        _unitOfWork.Currencies.Add(currency);
        await _unitOfWork.SaveChanges();

        await _cache.Clear(CacheKeys.CurrencyPairs);
    }

    public async Task AddPair(CurrencyPair currencyPair)
    {
        _unitOfWork.CurrencyPairs.Add(currencyPair);
        await _unitOfWork.SaveChanges();

        await _cache.Clear(CacheKeys.CurrencyPairs);
    }
}
