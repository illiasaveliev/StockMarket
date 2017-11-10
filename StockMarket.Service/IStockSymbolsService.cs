using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StockMarket.Service.Dto;
using StockMarket.Service.Validation;

namespace StockMarket.Service
{
    public interface IStockSymbolsService
    {
        Task<ValidationResponse> UploadAsync(string user, string symbolName, Stream stream);

        Task<IEnumerable<StockSymbolInfo>> GetStocksAsync(string user);

        Task<IEnumerable<StockSymbolStatistics>> GetStockSymbolStatisticsAsync(string user, string symbolName, PriceType[] priceTypes, DateTime? from, DateTime? to);
    }
}