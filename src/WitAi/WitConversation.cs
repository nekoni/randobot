using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WitAi.Models;

namespace WitAi
{
    public class WitConversation<T>
    {
        WitClient client = null;
        string conversationId = null;
        T context = default(T);

        public delegate T MergeHandler(string conversationId, T context, Dictionary<string, List<Entity>> entities, double confidence);

        public delegate void SayHandler(string conversationId, T context, string msg, double confidence);

        public delegate T ActionHandler(string conversationId, T context, string action, Dictionary<string, List<Entity>> entities, double confidence);

        public delegate T StopHandler(string conversationId, T context);

        private MergeHandler merge;

        private SayHandler say;

        private ActionHandler action;

        private StopHandler stop;

        public WitConversation(string token, string conversationId, T initialContext, 
            MergeHandler merge, SayHandler say, ActionHandler action, StopHandler stop)
        {
            if (token == null || conversationId == null || merge == null || say == null || action == null)
            {
                throw new Exception("Please check WitConversation constructor parameters.");
            }

            this.client = new WitClient(token);
            this.conversationId = conversationId;
            this.merge = merge;
            this.say = say;
            this.action = action;
            this.stop = stop;
            this.context = initialContext;
        }

        public async Task<bool> SendMessageAsync(string q)
        {
            ConverseResponse response = await client.ConverseAsync(conversationId, q, context);
            return await RecurringConverseAsync(response);
        }

        private async Task<bool> RecurringConverseAsync(ConverseResponse prevResponse)
        {
            var doOneMoreStep = false;
            var tempContext = default(T);

            switch (prevResponse.TypeCode)
            {
                case MessageTypes.Merge:
                {
                    tempContext = this.merge.Invoke(conversationId, context, prevResponse.Entities, prevResponse.Confidence);
                    doOneMoreStep = true;
                    break;
                }
                    
                case MessageTypes.Msg:
                {
                    this.say.Invoke(conversationId, context, prevResponse.Msg, prevResponse.Confidence);
                    doOneMoreStep = true;
                    break;
                }
                    
                case MessageTypes.Action:
                {
                    tempContext = this.action.Invoke(conversationId, context, prevResponse.Action, prevResponse.Entities, prevResponse.Confidence);
                    doOneMoreStep = true;
                    break;
                }
                    
                case MessageTypes.Stop:
                {
                    tempContext = this.stop.Invoke(conversationId, context);
                    doOneMoreStep = false;
                    break;
                }
                    
                default:
                {
                    break;
                }
            }

            if (tempContext != null)
            {
                context = tempContext;
            }
                
            if (doOneMoreStep)
            {
                ConverseResponse response = await client.ConverseAsync(conversationId, null, context);
                return await RecurringConverseAsync(response);
            }

            return true;
        }
    }
}
