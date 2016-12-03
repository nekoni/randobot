using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Messenger.Client.Objects;
using Messenger.Client.Services;
using Microsoft.Extensions.Logging;
using RandoBot.Service.Repositories;
using RandoBot.Service.Services.Messenger;

namespace RandoBot.Service.Services
{
    /// <summary>
    /// The MessageProcessorService class.
    /// </summary>
    public class MessageProcessorService
    {
        private List<IMessageHandler> handlers = new List<IMessageHandler>();

        /// <summary>
        /// /// Initializes a new instance of the <see cref="MessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageSender">The message sender.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="pictureRepository">The picture repository.</param>
        /// <param name="redisService">The redis service.</param>
        public MessageProcessorService (
            ILogger<MessageProcessorService> logger,
            IMessengerMessageSender messageSender, 
            UserRepository userRepository, 
            PictureRepository pictureRepository,
            RedisService redisService)
        {
            this.Logger = logger;
            this.MessageSender = messageSender;
            this.UserRepository = userRepository;
            this.PictureRepository = pictureRepository;
            this.RedisService = redisService;

            this.handlers.Add(new HelpMessageHandler(this));
            this.handlers.Add(new TextMessageHandler(this));
            this.handlers.Add(new ImageMessageHandler(this));
        }

        /// <summary>
        /// The sender.
        /// </summary>
        public IMessengerMessageSender MessageSender { get; set; }

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger<MessageProcessorService> Logger { get; set; }

        /// <summary>
        /// The user repository.
        /// </summary>
        public UserRepository UserRepository { get; set; }

        /// <summary>
        /// The picture repository.
        /// </summary>
        public PictureRepository PictureRepository { get; set; }

        /// <summary>
        /// The redis service.
        /// </summary>
        public RedisService RedisService { get; set; }

        /// <summary>
        /// Processes a message;
        /// </summary>
        /// <param name="messageContainer"></param>
        /// <returns>True if the message was processed, otherwise false.</returns>
        public async Task<bool> ProcessMessageAsync(MessengerMessaging messageContainer)
        {
            var processed = false;

            await this.MessageSender.SendActionAsync(MessengerSenderAction.MarkSeen, messageContainer.Sender);

            foreach (var handler in this.handlers)
            {
                try
                {
                    var result = await handler.HandleMessageAsync(messageContainer);
                
                    if (result)
                    {
                        processed = true;
                        break;
                    }
                }
                catch(Exception ex)
                {
                    this.Logger.LogError("Exception: {0}", ex.ToString());
                }
            }

            //TODO: This should be done in a scheduled worker thread or background process.
            await this.CleanPicturesAsync();

            return processed;
        }

        private async Task CleanPicturesAsync()
        {
            try
            {
                await this.PictureRepository.DeleteAsync();
            }
            catch(Exception ex)
            {
                this.Logger.LogError("Exception: {0}", ex.ToString());
            }
        }
    }
}