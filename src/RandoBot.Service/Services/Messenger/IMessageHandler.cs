using System.Threading.Tasks;
using Messenger.Client.Objects;

namespace RandoBot.Service.Services.Messenger
{
    /// <summary>
    /// The contact endpoint.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="messageContainer">The message container to process.</param>
        /// <returns>True the message was processed otherwise false.</returns>
        Task<bool> HandleMessageAsync(MessengerMessaging messageContainer);
    }
}