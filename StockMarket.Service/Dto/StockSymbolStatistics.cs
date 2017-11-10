using System;

namespace StockMarket.Service.Dto
{
    public class StockSymbolStatistics
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public double Avg { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public double Median { get; set; }

        public double Percentile95 { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }

        public double High { get; set; }

        public double Low { get; set; }
    }
}