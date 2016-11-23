namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Represents a link button.
    /// </summary>
    public class MessengerLinkButton : MessengerButtonBase
    {
        /// <summary>
        /// Initializes a new instance of <see ref="MessengerLinkButton" /> class.
        /// </summary>
        public MessengerLinkButton(string title, string url) 
            : base("web_url", title)
        {
            this.Url = url;
        }

        /// <summary>
        /// The link URL.
        /// </summary>
        public string Url { get; private set; }
    }
}