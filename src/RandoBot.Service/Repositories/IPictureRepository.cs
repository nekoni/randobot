using System.Threading.Tasks;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The picture repository interface.
    /// </summary>
    public interface IPictureRepository
    {
        /// <summary>
        /// Gets a random picture URL.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The URL of the picture.</returns>
        Task<string> GetRandomAsync(string userId);

        /// <summary>
        /// Inserts a picture based on the original URL.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="originalUrl">The original URL.</param>
        Task InsertAsync(string userId, string originalUrl);
        
        /// <summary>
        /// Deletes the dispatched pictures.
        /// </summary>
        Task DeleteAsync();
    }
}
