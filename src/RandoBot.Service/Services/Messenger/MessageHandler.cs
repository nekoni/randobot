using System.Collections.Generic;
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
            await this.Processor.MessageSender.SendActionAsync(MessengerSenderAction.TypingOn, recipient);
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
            await this.Processor.MessageSender.SendAsync(response, recipient);
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
            await this.Processor.MessageSender.SendAsync(response, recipient);
        }

        /// <summary>
        /// Seds a button to the user.
        /// </summary>
        /// <param name="recipient">The messenger user.</param>
        /// <param name="text">The text.</param>
        /// <param name="buttons">The buttons.</param>
        /// <returns>A task.</returns>
        public async Task SendTextWithButtonsAsync(MessengerUser recipient, string text, IEnumerable<MessengerButtonBase> buttons)
        {
            var response = new MessengerMessage();
            response.Attachment = new MessengerAttachment();
            response.Attachment.Type = "template";
            response.Attachment.Payload = new MessengerPayload();
            response.Attachment.Payload.TemplateType = "button";
            response.Attachment.Payload.Text = text;
            response.Attachment.Payload.Buttons = new List<MessengerButton>();

            foreach (var button in buttons)
            {
                var payloadButton = new MessengerButton();
                
                if (button is MessengerLinkButton)
                {
                    var linkButton = button as MessengerLinkButton;
                    payloadButton.Url = linkButton.Url;
                    payloadButton.Title = linkButton.Title;
                    payloadButton.Type = linkButton.Type;
                }

                if (button is MessengerChatButton)
                {
                    var chatButton = button as MessengerChatButton;
                    payloadButton.Payload = chatButton.Payload;
                    payloadButton.Title = chatButton.Title;
                    payloadButton.Type = chatButton.Type;
                }

                response.Attachment.Payload.Buttons.Add(payloadButton);
            }

            await this.Processor.MessageSender.SendAsync(response, recipient);
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