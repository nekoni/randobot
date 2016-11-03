using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RandoBot.Service.Models
{
    /// <summary>
    /// The user class.
    /// </summary>
    public class User : Entity
    {
        /// <summary>
        /// The first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last activity date time.
        /// </summary>
        public DateTime LastActivity { get; set; }
    }
}
