using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Client.Objects;
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
            // var witConversation = new WitConversation<ConversationContext>(
            //     witAiToken, 
            //     conversationContext.Id, 
            //     conversationContext, 
            //     this.DoMerge, 
            //     this.DoSay, 
            //     this.DoAction, 
            //     this.DoStop);

            // var result = await witConversation.SendMessageAsync(message.Text);
            
            var witClient = new WitAi.WitClient(witAiToken);
            var response = await witClient.GetMessageAsync(message.Text);

            if (response.Entities != null)
            {
                if (response.Entities.Count == 0)
                {
                    var buttons = new List<MessengerButtonBase>();
                    buttons.Add(new MessengerLinkButton("help web", "http://rando-bot.com"));
                    buttons.Add(new MessengerChatButton("help", "help"));

                    await this.SendTextWithButtonsAsync(sender, "I didn't quite get that, I'm a still a bit silly ATM :/" , buttons);
                }
                else
                {
                    foreach(var entity in response.Entities)
                    {
                        foreach (var value in entity.Value)
                        {
                            if (value.Confidence > 0.5)
                            {
                                var intent = value.Value.ToString();

                                switch(intent)
                                {
                                    case "Greetings":
                                        await this.SendTextAsync(sender, "Hi :)" , 1000);    
                                        break;
                                    case "Feeling":
                                    case "Feelings":
                                        await this.SendTextAsync(sender, "I'm fine thanks! :)" , 2000);    
                                        break;
                                    case "Identity":
                                        await this.SendTextAsync(sender, "I'm rando bot!" , 2000);    
                                        break;
                                    default :
                                        await this.SendTextAsync(sender, "I didn't quite get that, I'm a still a bit silly ATM :/" , 3000);
                                        break;   
                                }

                                return true;
                            }
                            else
                            {
                                await this.SendTextAsync(sender, "I didn't quite get that, I'm a still a bit silly ATM :/" , 3000);    
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        private ConversationContext DoMerge(string conversationId, ConversationContext context, Dictionary<string, List<Entity>> entities, double confidence)
        {
            return context;
        }

        private void DoSay(string conversationId, ConversationContext context, string msg, double confidence)
        {
            var message = msg;
        }

        private ConversationContext DoAction(string conversationId, ConversationContext context, string action, Dictionary<string, List<Entity>> entities, double confidence)
        {
            if (entities != null)
            {
                foreach (var entry in entities)
                {
                    if (entry.Key == "intent")
                    {
                        foreach (var entity in entry.Value)
                        {
                            var token = entity.Value.FirstOrDefault();                        
                        }
                    }
                }
            }
            
            return context;
        }

        private ConversationContext DoStop(string conversationId, ConversationContext context)
        {
            return context;
        }
    }
}