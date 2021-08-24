using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PuttPutt.Models
{
    /// <summary>
    /// Model holding data around participants records
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// Mongo unique ID for entry
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Discord user tied to this record
        /// </summary>
        public DiscordUser User { get; set; }
        
        /// <summary>
        /// Discord server record is generated for
        /// </summary>
        public DiscordGuild Server { get; set; }
        
        /// <summary>
        /// Current score
        /// </summary>
        public int Score { get; set; }
    }
}
