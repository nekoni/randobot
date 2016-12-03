using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RandoBot.Service.Services
{
    /// <summary>
    /// Redis service.
    /// </summary>
    public class RedisService
    {
        private ILogger<RedisService> logger;

        private IDatabase database;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisService"/> class.
        /// </summary>
        public RedisService(ILogger<RedisService> logger)
        {
            this.logger = logger;

            var server = Environment.GetEnvironmentVariable("REDIS_SERVER");
            if (server == null) 
            {
                throw new Exception("Cannot find REDIS_SERVER in this env.");
            }

            var serverIp = this.GetServerIp(server);

            var connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
            if (connectionString == null) 
            {
                throw new Exception("Cannot find REDIS_CONNECTION_STRING in this env.");
            }

            var target = serverIp + connectionString;
            logger.LogDebug(target);
            var connection = ConnectionMultiplexer.Connect(target);
            this.database = connection.GetDatabase();
        }

        /// <summary>
        /// Stores a key value pair.
        /// </summary>
        public async Task<T> FindOrCreateAsync<T>(string key, T value)
            where T : class
        {
            var obj = await FindAsync<T>(key);

            if (obj == null)
            {
                var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(value));
                var result = await database.StringSetAsync(key, json);

                if (result != true)
                {
                    throw new Exception("Value cannot be created");
                }
            }
            
            return obj;
        }


        /// <summary>
        /// Finds a value for a specific key.
        /// </summary>
        public async Task<T> FindAsync<T>(string key) 
            where T : class
        {
            var json = await this.database.StringGetAsync(key);
            return json.HasValue 
                ? await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(json))
                : null;
        }

        /// <summary>
        /// Deletes a value for a specific key.
        /// </summary>
        public async Task DeleteAsync<T>(string key)
            where T : class
        {
            var obj = await FindAsync<T>(key);

            if (obj != null) 
            {
                var result = await database.KeyDeleteAsync(key);

                if (result != true)
                {
                    throw new Exception("Value cannot be deleted");
                }
            }
        }

        private string GetServerIp(string server)
        {
            var addresslist = Dns.GetHostAddressesAsync(server).Result;

            foreach (IPAddress address in addresslist)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address.ToString();
                }
            }

            return null;
        }
    }
}
