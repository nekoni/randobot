using System;

namespace RandoBot.Service.Models
{
    /// <summary>
    /// The picture class.
    /// </summary>
    public class Picture : Entity
    {
        /// <summary>
        /// The public identifier.
        /// </summary>
        public string PublicId { get; set; }

        /// <summary>
        /// Marks this record to be deleted.
        /// </summary>
        public DateTime Delete { get; set; }
    }
}
