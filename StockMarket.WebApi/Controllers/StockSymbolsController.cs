using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockMarket.Service;
using StockMarket.Service.Dto;
using StockMarket.Service.Validation;
using StockMarket.WebApi.Models;

namespace StockMarket.WebApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/StockSymbols")]
    public class StockSymbolsController : Controller
    {
        private readonly IStockSymbolsService stockSymbolsService;

        public StockSymbolsController(IStockSymbolsService stockSymbolsService)
        {
            this.stockSymbolsService = stockSymbolsService;
        }

        private string UserName => User.Claims.FirstOrDefault(c => c.Type.Contains("emailaddress"))?.Value ?? User.Identity.Name;

        /// <summary>
        /// Gets available stocks with the most recent update timestamp
        /// </summary>
        /// <remarks>
        /// Sample response:
        /// 
        /// [
        ///     {
        ///         "name": "Goog",
        ///         "mostRecentDate": "2017-10-18T00:00:00"
        ///     }
        /// ]    
        /// </remarks>
        /// <returns>Returns the list of available stocks</returns>
        [HttpGet]
        public async Task<IEnumerable<StockSymbolInfo>> Get()
        {
            return await stockSymbolsService.GetStocksAsync(UserName);
        }

        /// <summary>
        /// Query individual stock symbol for the following stats: min, avg, max, median, 95th percentile for each price type (OHLC)
        /// </summary>
        /// <remarks>
        /// The  following query parameters can be specified:
        /// 
        ///     Symbol - the name of the stock symbol;
        /// 
        ///     PriceTypes - a subset of price types that must be included in the result (e.g. if only open and close must be returned);
        /// 
        ///     From - from date;
        /// 
        ///     To - to date;
        /// 
        /// </remarks>
        /// <param name="symbolStatisticsQuery">Query parameters</param>
        /// <returns>Returns statistics for individual stock symbol</returns>
        [HttpGet("statistics")]
        public async Task<IEnumerable<StockSymbolStatistics>> GetStatistics(SymbolStatisticsQuery symbolStatisticsQuery)
        {
            return await stockSymbolsService.GetStockSymbolStatisticsAsync(UserName, symbolStatisticsQuery.Symbol, symbolStatisticsQuery.PriceTypes.ToArray(), symbolStatisticsQuery.From, symbolStatisticsQuery.To);
        }

        /// <summary>
        /// Uploads market data for individual stock symbol
        /// </summary>
        /// <remarks>
        /// The following data is required:
        /// 
        ///     SymbolName - the name of the stock symbol, e.g. Goog
        /// 
        ///     File - CSV file with market data.
        /// </remarks>
        /// <param name="stockSymbol">Market data</param>
        /// <returns>Upload status and validation messages in case of incorrect data</returns>
        [HttpPost]
        public async Task<IActionResult> Post(StockSymbolsUploadModel stockSymbol)
        {
            using (Stream fileStream = stockSymbol.File.OpenReadStream())
            {
                ValidationResponse result = await stockSymbolsService.UploadAsync(UserName, stockSymbol.SymbolName, fileStream);
                if (!result.IsValid)
                {
                    return BadRequest(result.ValidationResults);
                }
            }

            return Ok("Stocks data was uploaded successfully.");
        }
    }
}