using System;
using RandoBot.Service.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The user repository class.
    /// </summary>
    public class UserRepository : MongoRepository
    {
        private IMongoCollection<User> collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        public UserRepository ()
        {
            this.collection = this.Db.GetCollection<User>("Users");
        }

        /// <summary>
        /// Get the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user if found, otherwise null.</returns>
        public async Task<User> GetAsync(string userId)
        {
            var users = await this.collection.FindAsync(f => f.UserId == userId);
            return await users.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Insert the user into the database.
        /// </summary>
        /// <param name="user">The user.</param>
        public async Task<User> InsertAsync(User user)
        {
            user.Created = DateTime.UtcNow;
            user.LastActivity = user.Created;
            await this.collection.InsertOneAsync(user);

            return user;
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        public async Task<User> UpdateAsync(User user)
        {
            user.LastActivity = DateTime.UtcNow;
            await this.collection.UpdateOneAsync(
                Builders<User>.Filter.Eq("UserId", user.UserId), 
                Builders<User>.Update.Set("LastActivity", user.LastActivity));

            return user;
        }
    }
}
