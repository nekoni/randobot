using System;
using MongoDB.Driver;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// Base class for mongo db repositories.
    /// </summary>
    public abstract class MongoRepository
    {
        /// <summary>
        /// The db client.
        /// </summary>
        protected MongoClient Client { get; private set; }

        /// <summary>
        /// The db.
        /// </summary>
        protected IMongoDatabase Db { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        public MongoRepository()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
            var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME");

            this.Client = new MongoClient(connectionString);
            this.Db = this.Client.GetDatabase(databaseName);
        }
    }
}
