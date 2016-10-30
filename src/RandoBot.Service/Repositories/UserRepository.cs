using System;
using RandoBot.Service.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The user repository class.
    /// </summary>
    public class UserRepository : MongoRepository, IUserRepository
    {
        /// <summary>
        /// Get the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user if found, otherwise null.</returns>
        public async Task<User> GetUserAsync(string userId)
        {
            var users = await this.Db.GetCollection<User>("Users").FindAsync(f => f.UserId == userId);
            return await users.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Insert the user into the database.
        /// </summary>
        /// <param name="user">The user.</param>
        public async Task<User> InsertUserAsync(User user)
        {
            user.Created = DateTime.UtcNow;
            await this.Db.GetCollection<User>("Users").InsertOneAsync(user);

            return user;
        }
    }
}
