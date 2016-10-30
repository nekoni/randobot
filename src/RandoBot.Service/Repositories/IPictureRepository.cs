using System.Threading.Tasks;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The picture repository.
    /// </summary>
    public interface IPictureRepository
    {
        /// <summary>
        /// Gets a random picture URL.
        /// </summary>
        /// <returns>The URL of the picture.</returns>
        Task<string> GetRandomPictureAsync();

        /// <summary>
        /// Creates a picture based on the original URL..
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="originalUrl">The original URL.</param>
        Task CreatePictureAsync(string userId, string originalUrl);
    }
}
