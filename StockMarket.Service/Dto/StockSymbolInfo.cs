using System;

namespace StockMarket.Service.Dto
{
    public class StockSymbolInfo
    {
        public StockSymbolInfo(string name, DateTime mostRecentDate)
        {
            Name = name;
            MostRecentDate = mostRecentDate;
        }

        public string Name { get; }

        public DateTime MostRecentDate { get; }
    }
}