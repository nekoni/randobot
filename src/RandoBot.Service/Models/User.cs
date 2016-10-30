using System;

namespace RandoBot.Service.Models
{
    /// <summary>
    /// The user class.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The user identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The created datetime.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
