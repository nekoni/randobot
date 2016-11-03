using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using RandoBot.Service.Models;

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

        private IMongoCollection<Picture> collection;

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

            this.collection = this.Db.GetCollection<Picture>("Pictures");
        }

        /// <summary>
        /// Inserts a new picture.
        /// </summary>
        /// <param name="userId">The user identity.</param>
        /// <param name="originalUrl">The original URL.</param>
        public async Task InsertAsync(string userId, string originalUrl)
        {
            var picture = new Picture
            {
                Created = DateTime.UtcNow,
                PublicId = $"{Guid.NewGuid().ToString()}",
                UserId = userId,
                Delete = DateTime.MaxValue
            };

            await this.collection.InsertOneAsync(picture);

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

            var resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                await this.collection.DeleteOneAsync(Builders<Picture>.Filter.Eq("Id", picture.Id));

                throw new Exception(resultContent);
            }
        }

        /// <summary>
        /// Gets a random picture.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The URL of the picture.</returns>
        public async Task<string> GetRandomAsync(string userId)
        {
            var count = (int)await this.collection.CountAsync(Builders<Picture>.Filter.Empty);
            var randomNumber = new Random().Next(0, count - 1);
            var options = new FindOptions<Picture> { Skip = randomNumber, Limit = 1 };
            var pictures = await this.collection
                .FindAsync(Builders<Picture>.Filter.Ne(p => p.UserId, userId), options);
            
            Picture picture = null;
            foreach (var p in await pictures.ToListAsync())
            {
                if (p.Delete == DateTime.MaxValue)
                {
                    picture = p;
                    break;
                }
            }

            var publicId = "sample";
            
            if (picture != null) 
            {
                publicId = picture.PublicId;
                await this.collection.UpdateOneAsync(
                        Builders<Picture>.Filter.Eq(p => p.Id, picture.Id),
                        Builders<Picture>.Update.Set("Delete", DateTime.UtcNow)
                );
            }
            
            return $"http://res.cloudinary.com/{this.cloudName}/image/upload/{publicId}";
        }

        /// <summary>
        /// Deletes the dispatched pictures.
        /// </summary>
        public async Task DeleteAsync()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-30);
            var filter = Builders<Picture>.Filter.Empty;

            var picturesToDelete = await this.collection.FindAsync(filter);
            foreach (var pictureToDelete in picturesToDelete.ToList())
            {
                if (pictureToDelete.Delete > threshold)
                {
                    continue;
                }

                var url = $"https://api.cloudinary.com/v1_1/{this.cloudName}/image/destroy";

                var timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                var stringToSign = $"public_id={pictureToDelete.PublicId}&timestamp={timestamp}{this.apiSecret}";
                var signature = SHA1Util.SHA1HashStringForUTF8String(stringToSign);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_key", apiKey),
                    new KeyValuePair<string, string>("timestamp", timestamp.ToString()),
                    new KeyValuePair<string, string>("signature", signature),
                    new KeyValuePair<string, string>("public_id", pictureToDelete.PublicId),
                });

                var client = new HttpClient();
                var result = await client.PostAsync(url, content);

                var resultContent = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(resultContent);
                }

                var deleteFilter = Builders<Picture>.Filter.Eq("PublicId", pictureToDelete.PublicId);
                await this.collection.DeleteOneAsync(deleteFilter);
            }
        }
    }
}
