using System.Collections.Generic;
using System.IO;
using StockMarket.Domain;
using StockMarket.Service.Validation;

namespace StockMarket.Service
{
    public interface ICsvReader
    {
        ValidationResponse<IEnumerable<StockSymbol>> ReadStockSymbols(string user, string symbolName, Stream data);
    }
}