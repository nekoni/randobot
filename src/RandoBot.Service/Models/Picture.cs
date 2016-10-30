using System;

namespace RandoBot.Service.Models
{
    public class Picture
    {
        /// <summary>
        /// The user identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The created datetime.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The public identifier.
        /// </summary>
        public string PublicId { get; set; }
    }
}
