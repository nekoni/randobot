using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Client.Objects;
using WitAi;
using WitAi.Models;

namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Handles a text message.
    /// </summary>
    public class TextMessageHandler : MessageHandler, IMessageHandler
    {
        private string witAiToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessageHandler" /> class.
        /// </summary>
        /// <param name="processor">The message processor.</param>
        public TextMessageHandler (MessageProcessorService processor) 
            : base(processor)
        {
            this.witAiToken = Environment.GetEnvironmentVariable("WITAI_TOKEN");
            if (this.witAiToken == null) 
            {
                throw new Exception("Cannot find WITAI_TOKEN in this env.");
            }            
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

            var conversationContext = await this.Processor.RedisService.FindOrCreateAsync(sender.Id, new ConversationContext { Id = Guid.NewGuid().ToString() });
            var witConversation = new WitConversation<ConversationContext>(
                witAiToken, 
                conversationContext.Id, 
                conversationContext, 
                this.DoMerge, 
                this.DoSay, 
                this.DoAction, 
                this.DoStop);

            await this.SendTextAsync(sender, "let's exchange some pictures! Send me yours first :)", 1000);

            return true;
        }

        private ConversationContext DoMerge(string conversationId, ConversationContext context, Dictionary<string, List<Entity>> entities, double confidence)
        {
            return context;
        }

        private void DoSay(string conversationId, ConversationContext context, string msg, double confidence)
        {
        }

        private ConversationContext DoAction(string conversationId, ConversationContext context, string action, Dictionary<string, List<Entity>> entities, double confidence)
        {
            return context;
        }

        private ConversationContext DoStop(string conversationId, ConversationContext context)
        {
            return context;
        }
    }
}