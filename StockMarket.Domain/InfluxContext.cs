using System.Collections.Generic;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;

namespace StockMarket.Domain
{
    public class InfluxContext : IInfluxContext
    {
        private readonly string connectionString;
        private readonly string user;
        private readonly string password;

        public InfluxContext(string connectionString, string databaseName, string user, string password)
        {
            this.connectionString = connectionString;
            this.user = user;
            this.password = password;
            DatabaseName = databaseName;
        }

        public string DatabaseName { get; }

        public async Task<InfluxDBClient> GetDatabaseClient()
        {
            InfluxDBClient client = new InfluxDBClient(connectionString, user, password);
            List<string> databases = await client.GetInfluxDBNamesAsync();
            if (!databases.Contains(DatabaseName))
            {
                await client.CreateDatabaseAsync(DatabaseName);
            }

            return client;
        }
    }
}