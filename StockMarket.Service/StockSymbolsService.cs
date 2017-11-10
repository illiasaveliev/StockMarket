using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StockMarket.Domain;
using StockMarket.Service.Dto;
using StockMarket.Service.Extensions;
using StockMarket.Service.Validation;

namespace StockMarket.Service
{
    public class StockSymbolsService : IStockSymbolsService
    {
        private readonly ICsvReader csvReader;

        private readonly IStockSymbolsRepository stockSymbolsRepository;

        public StockSymbolsService(ICsvReader csvReader, IStockSymbolsRepository stockSymbolsRepository)
        {
            this.csvReader = csvReader;
            this.stockSymbolsRepository = stockSymbolsRepository;
        }

        public async Task<ValidationResponse> UploadAsync(string user, string symbolName, Stream stream)
        {
            ValidationResponse<IEnumerable<StockSymbol>> readerResult = csvReader.ReadStockSymbols(user, symbolName, stream);

            if (readerResult.IsValid)
            {
                DateTime? mostRecentDate = (await stockSymbolsRepository.GetByUserAndSymbolAsync(user, symbolName))
                    .OrderByDescending(s => s.Date).Select(s => s.Date).FirstOrDefault();

                DateTime? newSymbolsStartDate = readerResult.Result.OrderByDescending(r => r.Date).Select(s => s.Date).FirstOrDefault();

                if (newSymbolsStartDate > mostRecentDate)
                {
                    await stockSymbolsRepository.AddRangeAsync(readerResult.Result);
                }
                else
                {
                    return new ValidationResponse(new[] { new ValidationResult(0, $"You already have data for this period. Please upload new values starting from {mostRecentDate}.") });
                }
            }

            return new ValidationResponse(readerResult.ValidationResults);
        }

        public async Task<IEnumerable<StockSymbolInfo>> GetStocksAsync(string user)
        {
            IEnumerable<StockSymbol> stockSymbols = await stockSymbolsRepository.GetByUserAsync(user);
            return stockSymbols.GroupBy(s => s.Name).Select(
                s => new StockSymbolInfo(s.Key, s.OrderByDescending(i => i.Date).First().Date)).ToArray();
        }

        public async Task<IEnumerable<StockSymbolStatistics>> GetStockSymbolStatisticsAsync(string user, string symbolName, PriceType[] priceTypes, DateTime? from, DateTime? to)
        {
            return (await stockSymbolsRepository.GetByUserAndSymbolAsync(user, symbolName, from, to)).Select(symbol => CreateStatistics(symbol, priceTypes)).ToList();
        }

        private static StockSymbolStatistics CreateStatistics(StockSymbol symbol, PriceType[] priceTypes)
        {
            List<double> prices = GetPrices(symbol, priceTypes).ToList();
            return new StockSymbolStatistics
            {
                Name = symbol.Name,
                Date = symbol.Date,
                Min = prices.Min(p => p),
                Avg = prices.Average(p => p),
                Max = prices.Max(p => p),
                Median = prices.Percentile(0.5),
                Percentile95 = prices.Percentile(0.95),
                Open = symbol.Open,
                Close = symbol.Close,
                High = symbol.High,
                Low = symbol.Low
            };
        }

        private static IEnumerable<double> GetPrices(StockSymbol stockSymbol, PriceType[] priceTypes)
        {
            if (!priceTypes.Any())
            {
                return new[] { stockSymbol.Open, stockSymbol.Close, stockSymbol.High, stockSymbol.Low };
            }

            List<double> prices = new List<double>();

            if (priceTypes.Contains(PriceType.Open))
            {
                prices.Add(stockSymbol.Open);
            }
            if (priceTypes.Contains(PriceType.Close))
            {
                prices.Add(stockSymbol.Close);
            }
            if (priceTypes.Contains(PriceType.High))
            {
                prices.Add(stockSymbol.High);
            }
            if (priceTypes.Contains(PriceType.Low))
            {
                prices.Add(stockSymbol.Low);
            }

            return prices;
        }
    }
}