﻿using System;

namespace StockMarket.Domain
{
    public class StockSymbol
    {
        public string Name { get; set; }

        public string UserName { get; set; }

        public DateTime Date { get; set; }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }

        public double Volume { get; set; }
    }
}