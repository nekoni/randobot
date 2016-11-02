using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RandoBot.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RandoBot.Service.Repositories
{
    /// <summary>
    /// The picture repository.
    /// </summary>
    public class PictureRepository : MongoRepository, IPictureRepository
    {
        private string cloudName;

        private string apiKey;

        private string apiSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureRepository"/> class.
        /// </summary>
        public PictureRepository() : base()
        {
            this.cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_NAME");
            if (this.cloudName == null) 
            {
                throw new Exception("Cannot find CLOUDINARY_NAME in this env.");
            }

            this.apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            if (this.apiKey == null) 
            {
                throw new Exception("Cannot find CLOUDINARY_API_KEY in this env.");
            }

            this.apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
            if (this.apiSecret == null) 
            {
                throw new Exception("Cannot find CLOUDINARY_API_SECRET in this env.");
            }
        }

        /// <summary>
        /// Creates a new picture.
        /// </summary>
        /// <param name="userId">The user identity.</param>
        /// <param name="originalUrl">The original URL.</param>
        public async Task CreatePictureAsync(string userId, string originalUrl)
        {
            var picture = new Picture
            {
                Created = DateTime.UtcNow,
                PublicId = $"{Guid.NewGuid().ToString()}",
                UserId = userId
            };

            await this.Db.GetCollection<Picture>("Pictures").InsertOneAsync(picture);

            var url = $"https://api.cloudinary.com/v1_1/{this.cloudName}/image/upload";

            var timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var stringToSign = $"public_id={picture.PublicId}&timestamp={timestamp}{this.apiSecret}";
            var signature = SHA1Util.SHA1HashStringForUTF8String(stringToSign);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("file", originalUrl),
                new KeyValuePair<string, string>("api_key", apiKey),
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),
                new KeyValuePair<string, string>("signature", signature),
                new KeyValuePair<string, string>("public_id", picture.PublicId),
            });

            var client = new HttpClient();
            var result = await client.PostAsync(url, content);

            string resultContent = result.Content.ReadAsStringAsync().Result;
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(resultContent);
            }
        }

        /// <summary>
        /// Gets a random picture.
        /// </summary>
       /// <param name="userId">The user identifier.</param>
        /// <returns>The URL of the picture.</returns>
        public async Task<string> GetRandomPictureAsync(string userId)
        {
            var count = (int)await this.Db.GetCollection<Picture>("Pictures").CountAsync(Builders<Picture>.Filter.Empty);
            var randomNumber = new Random().Next(0, count - 1);
            var options = new FindOptions<Picture> { Skip = randomNumber, Limit = 1 };
            var pictures = await this.Db.GetCollection<Picture>("Pictures").FindAsync(Builders<Picture>.Filter.Empty, options);
            var picture = await pictures.FirstOrDefaultAsync();

            return $"http://res.cloudinary.com/{this.cloudName}/image/upload/{picture.PublicId}";
        }
    }
}
