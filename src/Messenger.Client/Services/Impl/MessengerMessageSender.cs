using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Messenger.Client.Config;
using Messenger.Client.Errors;
using Messenger.Client.Objects;
using Messenger.Client.Utilities;

namespace Messenger.Client.Services.Impl
{
    public class MessengerMessageSender : IMessengerMessageSender
    {
        private const String UrlTemplate = "https://graph.facebook.com/v2.6/me/messages?access_token={0}";

        private readonly HttpClient client;
        private readonly IMessengerSerializer serializer;

        public MessengerMessageSender(IMessengerSerializer serializer)
        {
            client = new HttpClient();
            this.serializer = serializer;
        }

        public Task<MessengerResponse> SendAsync(MessengerMessage message, MessengerUser recipient)
        {
            return SendAsync(message, recipient, MessengerConfig.AccessToken);
        }

        public Task<MessengerResponse> SendActionAsync(string senderAction, MessengerUser recipient)
        {
            return SendActionAsync(senderAction, recipient, MessengerConfig.AccessToken);
        }

        public async Task<MessengerResponse> SendActionAsync(string senderAction, MessengerUser recipient, String accessToken)
        {
            var url = String.Format(UrlTemplate, accessToken);
            var request = new MessengerSendActionRequest { Recipient = recipient, SenderAction = senderAction };
            var strings = serializer.Serialize(request);

            var content = new StringContent(strings);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            try
            {
                var response = await client.PostAsync(url, content);
                var result = new MessengerResponse
                {
                    Succeed = response.IsSuccessStatusCode,
                    RawResponse = await response.Content.ReadAsStringAsync()
                };

                return result;
            }
            catch (Exception exc)
            {
                throw new MessengerException(exc);
            }
        }

        public async Task<MessengerResponse> SendAsync(MessengerMessage message, MessengerUser recipient, String accessToken)
        {
            var url = String.Format(UrlTemplate, accessToken);
            var request = new MessengerSendMessageRequest { Recipient = recipient, Message = message };
            var strings = serializer.Serialize(request);

#if DEBUG
            System.Console.WriteLine(strings);
#endif
            /*if (message.Attachments != null && message.Attachments.Count > 0)
                throw new Exception(strings);*/

            var content = new StringContent(strings);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            try
            {
                var response = await client.PostAsync(url, content);
                var result = new MessengerResponse
                {
                    Succeed = response.IsSuccessStatusCode,
                    RawResponse = await response.Content.ReadAsStringAsync()
                };

                return result;
            }
            catch (Exception exc)
            {
                throw new MessengerException(exc);
            }
        }

    }
}
