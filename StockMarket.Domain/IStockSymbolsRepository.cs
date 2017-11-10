using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockMarket.Domain
{
    public interface IStockSymbolsRepository
    {
        Task<bool> AddRangeAsync(IEnumerable<StockSymbol> stockSymbols);

        Task<IEnumerable<StockSymbol>> GetByUserAsync(string user);

        Task<IEnumerable<StockSymbol>> GetByUserAndSymbolAsync(string user, string symbolName, DateTime? from = null, DateTime? to = null);
    }
}