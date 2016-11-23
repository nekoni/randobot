namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// Represents a base button.
    /// </summary>
    public abstract class MessengerButtonBase
    {
        /// <summary>
        /// Initializes a new instance of <see ref="MessengerButtonBase" /> class.
        /// </summary>
        public MessengerButtonBase(string type, string title)
        {
            this.Type = type;
            this.Title = title;
        }

        /// <summary>
        /// The button type.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The button title.
        /// </summary>
        public string Title { get; private set; }
    }
}