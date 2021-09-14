using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace PuttPutt.Models
{
    /// <summary>
    /// Model holds an archive entry for a golf season. 1 Entry per particpant, per season
    /// </summary>
    class Archive
    {
        /// <summary>
        /// Mongo unique ID for entry
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Archived name, used to group entries for seasons
        /// </summary>
        public string ArchiveName { get; set; }

        /// <summary>
        /// DiscordGuild id record is part of
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// Entry for the season archive as of time of archival
        /// </summary>
        public List<Participant> Participant { get; set; }

        [BsonExtraElements]
        private BsonDocument CatchAll { get; set; }
    }
}
