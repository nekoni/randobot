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
            if (connectionString == null) 
            {
                throw new Exception("Cannot find MONGODB_URI in this env.");
            }

            var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME");            
            if (databaseName == null) 
            {
                throw new Exception("Cannot find MONGODB_NAME in this env.");
            }
            
            this.Client = new MongoClient(connectionString);
            this.Db = this.Client.GetDatabase(databaseName);
        }
    }
}
