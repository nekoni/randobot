using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Messenger.Client.Objects;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Handles an image message.
    /// </summary>
    public class ImageMessageHandler : MessageHandler, IMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMessageHandler" /> class.
        /// </summary>
        /// <param name="processor">The message processor.</param>
        public ImageMessageHandler (MessageProcessorService processor) 
            : base (processor)
        {
        }
        
        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="messageContainer">The message container.</param>
        /// <returns>The result of the operation.</returns>
        public async Task<bool> HandleMessageAsync(MessengerMessaging messageContainer)
        {
            var message = messageContainer.Message;
            var sender = messageContainer.Sender;

            var user = await this.GetUserAsync(sender);

            if (user == null)
            {
                return false;
            }

            var attachement = message.Attachments?.FirstOrDefault();
            if (attachement?.Type != "image")
            {
                return false;
            }

            await this.Processor.PictureRepository.InsertAsync(sender.Id, attachement.Payload.Url);

            var pictureUrl = await this.Processor.PictureRepository.GetRandomAsync(user.UserId);
            var location = await this.GetLocationFromPictureAsync(pictureUrl);

            if (location != null)
            {
                await this.SendTextAsync(sender, location);
            }

            await this.SendPictureAsync(sender, pictureUrl);

            return true;
        }

        private async Task<string> GetLocationFromPictureAsync(string pictureUrl)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(pictureUrl))
                {
                    response.EnsureSuccessStatusCode();

                    using (var inputStream = await response.Content.ReadAsStreamAsync())
                    {
                        var directories = ImageMetadataReader.ReadMetadata(inputStream);
                        foreach (var directory in directories)
                        {
                            this.Processor.Logger.LogDebug(directory.Name);
                        }

                        var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
        
                        if (gps != null)
                        {
                            var location = gps.GetGeoLocation();
                            return $"lat: {location.Latitude} long: {location.Longitude}";
                        }
                    }
                }
            }

            return null;
        }
    }
}