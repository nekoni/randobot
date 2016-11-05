using Messenger.Client.Objects;
using Newtonsoft.Json;

namespace Messenger.Client.Services.Impl
{
    internal class MessengerSendActionRequest
    {
        public MessengerUser Recipient { get; set; }

        [JsonProperty("sender_action")]
        public string SenderAction { get; set; }
    }
}