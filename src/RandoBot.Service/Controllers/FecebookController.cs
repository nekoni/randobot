using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Messenger.Client.Objects;
using Messenger.Client.Services;
using Messenger.Client.Services.Impl;
using Messenger.Client.Utilities;
using RandoBot.Service.Repositories;
using RandoBot.Service.Models;

namespace RandoBot.Service.Controllers
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
                foreach (var messaging in entry.Messaging.Where(m => m.Message != null && m.Sender.Id != null))
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
                if (messaging.Message.Text == "help")
                {
                    response.Text = "send me a nice picture :)";
                }
                else
                {
                    var user = await this.userRepository.GetAsync(messaging.Sender.Id);
                    if (user == null)
                    {
                        var profileResponse = await new MessengerProfileProvider(new JsonMessengerSerializer()).GetUserProfileAsync(messaging.Sender.Id);
                        var profile = profileResponse.Result;

                        if (!string.IsNullOrEmpty(profile.FirstName))
                        {
                            user = new User
                            {
                                FirstName = profile.FirstName,
                                UserId = messaging.Sender.Id
                            };

                            user = await this.userRepository.InsertAsync(user);
                            response.Text = $"Hi {profile.FirstName}";    
                            await this.messageSender.SendAsync(response, messaging.Sender);

                            if (profile.Gender == "female")
                            {
                                response.Text = $"finally a girl ☺, boys pictures are so boring :/";
                                await this.messageSender.SendAsync(response, messaging.Sender);
                            }               
                        }
                    }
                    else
                    {
                        user = await this.userRepository.UpdateAsync(user);
                    }

                    var attachement = messaging.Message.Attachments?.FirstOrDefault();
                    if (attachement?.Type != "image")
                    {
                        response.Text = $"let's exchange some pictures! Send me yours first :)";
                    }
                    else
                    {
                        await this.pictureRepository.InsertAsync(messaging.Sender.Id, attachement.Payload.Url);

                        var pictureUrl = await this.pictureRepository.GetRandomAsync(user.UserId);
                        response.Attachment = new MessengerAttachment();
                        response.Attachment.Type = "image";
                        response.Attachment.Payload = new MessengerPayload();
                        response.Attachment.Payload.Url = pictureUrl;
                    }                    
                }
            }
            catch (Exception ex)
            {
                response.Text = "Oh boy, something went wrong :(";
                this.logger.LogError("Exception: {0}", ex.ToString());
            }
            finally
            {
                await this.messageSender.SendAsync(response, messaging.Sender);
            }

            try
            {
                await this.pictureRepository.DeleteAsync();
            }
            catch(Exception ex)
            {
                this.logger.LogError("Exception: {0}", ex.ToString());
            }
        }
    }
}
