using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StockMarket.Service.Dto;

namespace StockMarket.WebApi.Models
{
    public class SymbolStatisticsQuery
    {
        [Required]
        public string Symbol { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public List<PriceType> PriceTypes { get; set; }
    }
}