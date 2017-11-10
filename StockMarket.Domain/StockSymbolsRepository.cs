using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;

namespace StockMarket.Domain
{
    public class StockSymbolsRepository : IStockSymbolsRepository
    {
        private const string MeasureName = "StockSymbols";

        private readonly IInfluxContext influxContext;

        public StockSymbolsRepository(IInfluxContext influxContext)
        {
            this.influxContext = influxContext;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<StockSymbol> stockSymbols)
        {
            using (InfluxDBClient client = await influxContext.GetDatabaseClient())
            {
                List<InfluxDatapoint<InfluxValueField>> dataPoints = new List<InfluxDatapoint<InfluxValueField>>();
                foreach (StockSymbol stockSymbol in stockSymbols)
                {
                    InfluxDatapoint<InfluxValueField> point = new InfluxDatapoint<InfluxValueField>();
                    point.UtcTimestamp = stockSymbol.Date;
                    point.MeasurementName = MeasureName;
                    point.Precision = TimePrecision.Hours;
                    point.Tags.Add("SymbolName", stockSymbol.Name);
                    point.Fields.Add("Date", new InfluxValueField(stockSymbol.Date.ToString("MM-dd-yyyy")));
                    point.Fields.Add("UserName", new InfluxValueField(stockSymbol.UserName));
                    point.Fields.Add("SymbolName", new InfluxValueField(stockSymbol.Name));
                    point.Fields.Add("Open", new InfluxValueField(stockSymbol.Open));
                    point.Fields.Add("Close", new InfluxValueField(stockSymbol.Close));
                    point.Fields.Add("High", new InfluxValueField(stockSymbol.High));
                    point.Fields.Add("Low", new InfluxValueField(stockSymbol.Low));
                    point.Fields.Add("Volume", new InfluxValueField(stockSymbol.Volume));
                    dataPoints.Add(point);
                }

                return await client.PostPointsAsync(influxContext.DatabaseName, dataPoints);
            }
        }

        public async Task<IEnumerable<StockSymbol>> GetByUserAsync(string user)
        {
            using (InfluxDBClient client = await influxContext.GetDatabaseClient())
            {
                List<IInfluxSeries> result = await client.QueryMultiSeriesAsync(
                                                       influxContext.DatabaseName,
                                                       $"SELECT * FROM {MeasureName} WHERE UserName='{user}'",
                                                       TimePrecision.Hours);

                return result.Any() ? result.First().Entries.Select(CreateStockSymbol).ToList() : Enumerable.Empty<StockSymbol>();
            }
        }

        public async Task<IEnumerable<StockSymbol>> GetByUserAndSymbolAsync(string user, string symbolName, DateTime? from = null, DateTime? to = null)
        {
            using (InfluxDBClient client = await influxContext.GetDatabaseClient())
            {
                List<IInfluxSeries> result = await client.QueryMultiSeriesAsync(
                                                 influxContext.DatabaseName,
                                                 $"SELECT * FROM {MeasureName} WHERE UserName='{user}' AND SymbolName='{symbolName}'",
                                                 TimePrecision.Hours);

                return result.Any() ? result.First().Entries.Select(CreateStockSymbol).Where(
                    s => (!from.HasValue || s.Date >= from) && (!to.HasValue || s.Date <= to)).ToList() : Enumerable.Empty<StockSymbol>();
            }
        }

        private static StockSymbol CreateStockSymbol(dynamic entity)
        {
            return new StockSymbol
                       {
                           Close = double.Parse(entity.Close),
                           Open = double.Parse(entity.Open),
                           High = double.Parse(entity.High),
                           Low = double.Parse(entity.Low),
                           Name = entity.SymbolName,
                           Date = DateTime.Parse(entity.Date),
                           UserName = entity.UserName,
                           Volume = double.Parse(entity.Volume)
                       };
        }
    }
}