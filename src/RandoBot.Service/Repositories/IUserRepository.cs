using System.Threading.Tasks;
using RandoBot.Service.Models;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The interface of the user repository.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets an existing user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user if found otherwise null.</returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Inserts a new user.
        /// </summary>
        /// <param name="user">The user to insert.</param>
        /// <returns>The user.</returns>
        Task<User> InsertUserAsync(User user);
    }
}
