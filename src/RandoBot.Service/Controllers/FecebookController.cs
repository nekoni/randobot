using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Messenger.Client.Objects;
using Messenger.Client.Services;
using Messenger.Client.Services.Impl;
using Messenger.Client.Utilities;
using RandoBot.Service.Repositories;
using RandoBot.Service.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

        private readonly IUserRepository userRepository;

        private readonly IPictureRepository pictureRepository;

        private readonly ILogger<FacebookController> logger;

        /// <summary>
        /// Initializes a new instance of the FacebookController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageSender">The message sender.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="pictureRepository">The picture repository.</param>
        public FacebookController(ILogger<FacebookController> logger, IMessengerMessageSender messageSender, IUserRepository userRepository, IPictureRepository pictureRepository)
        {
            this.logger = logger;
            this.messageSender = messageSender;
            this.userRepository = userRepository;
            this.pictureRepository = pictureRepository;
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

            try
            {
                var user = await this.userRepository.GetUserAsync(messaging.Sender.Id);
                if (user == null)
                {
                    var profileResponse = await new MessengerProfileProvider(new JsonMessengerSerializer()).GetUserProfileAsync(messaging.Sender.Id);
                    user = new User
                    {
                        FirstName = profileResponse.Result.FirstName,
                        LastName = profileResponse.Result.LastName,
                        UserId = messaging.Sender.Id
                    };

                    if (string.IsNullOrEmpty(profileResponse.Result.FirstName))
                    {
                        return;
                    }

                    user = await this.userRepository.InsertUserAsync(user);

                    response.Text = $"Hi {user.FirstName}";
                    await this.messageSender.SendAsync(response, messaging.Sender);

                    response.Text = $"let's exchange some pictures! Send me yours first :)";
                }
                else
                {
                    var attachement = messaging.Message.Attachments?.FirstOrDefault();
                    if (attachement?.Type != "image")
                    {
                        response.Text = "Didn't get that, I'm a bit silly ATM, just send me a picture, por favour! :)";
                    }
                    else
                    {
                        await this.pictureRepository.CreatePictureAsync(messaging.Sender.Id, attachement.Payload.Url);

                        var pictureUrl = await this.pictureRepository.GetRandomPictureAsync(user.UserId);
                        response.Attachments = new List<MessengerAttachment>();
                        var attachment = new MessengerAttachment();
                        attachement.Payload.Url = pictureUrl;
                        response.Attachments.Add(attachement);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Text = "Oh boy, something went wrong :(";
                this.logger.LogError("Exception: {0}", ex.ToString());
            }

            await this.messageSender.SendAsync(response, messaging.Sender);
        }
    }
}
