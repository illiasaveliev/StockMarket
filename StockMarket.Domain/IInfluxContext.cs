using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;

namespace StockMarket.Domain
{
    public interface IInfluxContext
    {
        string DatabaseName { get; }

        Task<InfluxDBClient> GetDatabaseClient();
    }
}