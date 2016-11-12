using System.Threading.Tasks;
using Messenger.Client.Objects;
using Messenger.Client.Services.Impl;
using Messenger.Client.Utilities;
using RandoBot.Service.Models;

namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// MessengerHandler base class.
    /// </summary>
    public abstract class MessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler" /> class.
        /// </summary>
        /// <param name="processor">The message processor.</param>
        public MessageHandler (MessageProcessorService processor)
        {
            this.Processor = processor;
        }

        /// <summary>
        /// The message processor.
        /// </summary>
        protected MessageProcessorService Processor { get; private set; }

        /// <summary>
        /// Simulates typing.
        /// </summary>
        /// <param name="recipient">The messenger user.</param>
        /// <param name="duration">Duration.</param>
        /// <returns>A task.</returns>
        public async Task SimulateTypingAsync(MessengerUser recipient, int duration)
        {
            await this.Processor.Sender.SendActionAsync(MessengerSenderAction.TypingOn, recipient);
            await Task.Run(() => System.Threading.Thread.Sleep(duration));
        }

        /// <summary>
        /// Sends text to the user.
        /// </summary>
        /// <param name="recipient">The messenger user.</param>
        /// <param name="text">The text.</param>
        /// <param name="duration">The typing duration.</param>
        /// <returns>A task.</returns>
        public async Task SendTextAsync(MessengerUser recipient, string text, int duration)
        {
            await this.SimulateTypingAsync(recipient, duration);
            await this.SendTextAsync(recipient, text);
        }

        /// <summary>
        /// Sends text to the user.
        /// </summary>
        /// <param name="recipient">The messenger user.</param>
        /// <param name="text">The text.</param>
        /// <returns>A task.</returns>
        public async Task SendTextAsync(MessengerUser recipient, string text)
        {
            var response = new MessengerMessage { Text = text };
            await this.Processor.Sender.SendAsync(response, recipient);
        }

        /// <summary>
        /// Sends a picture to the user.
        /// </summary>
        /// <param name="recipient">The messenger user.</param>
        /// <param name="url">The picture url.</param>
        /// <returns>A task.</returns>
        public async Task SendPictureAsync(MessengerUser recipient, string url)
        {
            var response = new MessengerMessage();
            response.Attachment = new MessengerAttachment();
            response.Attachment.Type = "image";
            response.Attachment.Payload = new MessengerPayload();
            response.Attachment.Payload.Url = url;
            await this.Processor.Sender.SendAsync(response, recipient);
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="sender">The messenger user.</param>
        /// <returns>A task.</returns>
        public async Task<User> GetUserAsync(MessengerUser sender)
        {
            var user = await this.Processor.UserRepository.GetAsync(sender.Id);
            
            if (user == null)
            {
                var profileResponse = await new MessengerProfileProvider(
                    new JsonMessengerSerializer()).GetUserProfileAsync(sender.Id);
                var profile = profileResponse.Result;

                if (!string.IsNullOrEmpty(profile.FirstName))
                {
                    user = new User
                    {
                        FirstName = profile.FirstName,
                        UserId = sender.Id
                    };

                    user = await this.Processor.UserRepository.InsertAsync(user);
                    await this.SendTextAsync(sender, $"Hi {profile.FirstName}", 500);

                    if (profile.Gender == "female")
                    {
                        await this.SendTextAsync(sender, $"finally a girl ☺, boys pictures are so boring :/");
                    }               
                }
            }
            else
            {
                user = await this.Processor.UserRepository.UpdateAsync(user);
            }

            return user;
        }
    }
}