namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Represents a chat button.
    /// </summary>
    public class MessengerChatButton : MessengerButtonBase
    {
        /// <summary>
        /// Initializes a new instance of <see ref="MessengerChatButton" /> class.
        /// </summary>
        public MessengerChatButton(string title, string payload) 
            : base("postback", title)
        {
            this.Payload = payload;
        }

        /// <summary>
        /// The payload.
        /// </summary>
        public string Payload { get; set; }
    }
}