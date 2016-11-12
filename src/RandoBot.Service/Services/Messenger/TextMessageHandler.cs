using System.Linq;
using System.Threading.Tasks;
using Messenger.Client.Objects;

namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Handles a text message.
    /// </summary>
    public class TextMessageHandler : MessageHandler, IMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessageHandler" /> class.
        /// </summary>
        /// <param name="processor">The message processor.</param>
        public TextMessageHandler (MessageProcessorService processor) 
            : base(processor)
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
            if (attachement?.Type == "image" || string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            await this.SendTextAsync(sender, "let's exchange some pictures! Send me yours first :)", 1000);

            return true;
        }
    }
}