using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace PuttPutt.Models
{
    /// <summary>
    /// Class representing changes to a users entry
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Mongo unique ID for entry
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// UTC Time when modification occured
        /// </summary>
        public DateTime EventTimeUTC { get; set; }

        /// <summary>
        /// User current score after modification completed
        /// </summary>
        public int ScoreSnapshot { get; set; }

        /// <summary>
        /// Amount users score was modified by
        /// </summary>
        public int ScoreModifier { get; set; }

        /// <summary>
        /// Users score prior to event triggering
        /// </summary>
        public int PriorScore { get; set; }
    }
}
