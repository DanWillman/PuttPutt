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
        /// DiscordUser Id of particpant
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// DiscordGuild id record is part of
        /// </summary>
        public ulong ServerId { get; set; }
        
        /// <summary>
        /// Current score
        /// </summary>
        public int Score { get; set; }

        [BsonExtraElements]
        private BsonDocument CatchAll { get; set; }
    }
}
