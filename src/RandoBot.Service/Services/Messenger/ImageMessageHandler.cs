using System.Linq;
using System.Threading.Tasks;
using Messenger.Client.Objects;

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
            await this.SendPictureAsync(sender, pictureUrl);

            return true;
        }
    }
}