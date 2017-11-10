using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StockMarket.WebApi.Models
{
    public class StockSymbolsUploadModel
    {
        public IFormFile File { get; set; }

        [Required]
        public string SymbolName { get; set; }
    }
}