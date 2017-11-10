using System.Collections.Generic;
using System.Linq;

namespace StockMarket.Service.Extensions
{
    public static class EnumerableExtensions
    {
        public static double Percentile(this IEnumerable<double> values, double percentile)
        {
            int count = values.Count();
            if (count == 0)
            {
                return 0;
            }

            return values.OrderBy(v => v).Select((v, i) => new { v, i = i + 1 })
                .Select(v => new KeyValuePair<double, double>(v.v, v.i / (double)count))
                .First(p => p.Value >= percentile).Key;
        }
    }
}