using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Messenger.Client.Objects;
using Messenger.Client.Services;
using Messenger.Client.Services.Impl;
using Messenger.Client.Utilities;

namespace PriceTagCloud.Service.Controllers
{
    /// <summary>
    /// The FacebookController class.
    /// </summary>
    [Route("api/webhook")]
    public class FacebookController : Controller
    {
        private readonly IMessengerMessageSender messageSender;

        private readonly string verifyToken;

        /// <summary>
        /// Initializes a new instance of the FacebookController class.
        /// </summary>
        /// <param name="messageSender"></param>
        public FacebookController(IMessengerMessageSender messageSender)
        {
            this.messageSender = messageSender;
            this.verifyToken = Environment.GetEnvironmentVariable("VERIFY_TOKEN");
        }

        /// <summary>
        /// Validates the sender.
        /// </summary>
        /// <returns>The response.</returns>
        [HttpGet, Produces("text/html")]
        public IActionResult Validate()
        {
            var challenge = Request.Query["hub.challenge"];
            var verifyToken = Request.Query["hub.verify_token"];

            if (verifyToken.Any() && verifyToken.First() == this.verifyToken)
            {
                return Ok(challenge.First());
            }

            return Ok();
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="obj">The message object.</param>
        /// <returns>The response.</returns>
        [HttpPost]
        public async Task HandleMessage([FromBody] MessengerObject obj)
        {
            foreach (var entry in obj.Entries)
            {
                foreach (var messaging in entry.Messaging.Where(m => m.Message != null))
                {
                    await HandleEntry(messaging);
                }
            }
        }

        private async Task HandleEntry(MessengerMessaging messaging)
        {
            var response = new MessengerMessage();

            var profileResponse = await new MessengerProfileProvider(new JsonMessengerSerializer()).GetUserProfileAsync(messaging.Sender.Id);
            var value = new
            {
                FirstName = profileResponse.Result.FirstName,
                LastName = profileResponse.Result.LastName,
                Email = profileResponse.Result.Email
            };

            response.Text = $"Hi {value.FirstName}";
            await messageSender.SendAsync(response, messaging.Sender);

            response.Text = $"Send a picture to an internet stranger";
            await messageSender.SendAsync(response, messaging.Sender);

            //var attachement = messaging.Message.Attachments?.FirstOrDefault();
            //if (attachement?.Type != "image")
            //{
            //    response.Text = "Send me a picture to start a new search";
            //}
            //else
            //{                 

            //    var value = new
            //    {
            //        Id = Guid.NewGuid(),
            //        SendrId = messaging.Sender.Id,
            //        PayloadUrl = attachement.Payload.Url,
            //        Timestamp = messaging.Timestamp
            //    };

            //    sub.Publish("search", JsonConvert.SerializeObject(value));
            //    response.Text = $"Search in progress {value.Id}";
            //}

            //await messageSender.SendAsync(response, messaging.Sender);
        }
    }
}