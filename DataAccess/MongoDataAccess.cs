using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using PuttPutt.Models;

namespace PuttPutt.DataAccess
{
    /// <summary>
    /// Provides access to underlying data store in Mongo
    /// </summary>
    public class MongoDataAccess
    {
        private readonly string CollectionName = "scores";
        private readonly string ConnectionString = "mongodb://golf_mongo:27018";
        private readonly string DatabaseName = "shame_golf";

        private readonly IMongoCollection<Participant> collection;

        public MongoDataAccess()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);

            collection = database.GetCollection<Participant>(CollectionName);
            collection.Indexes.CreateOne(new CreateIndexModel<Participant>(Builders<Participant>.IndexKeys.Ascending(x => x.User.Username)));
        }

        /// <summary>
        /// Returns an unsorted list of all particpant entries for a specified server
        /// </summary>
        public List<Participant> GetParticipants(DiscordGuild server) => collection.Find(x => x.Server.Id == server.Id).ToList();

        /// <summary>
        /// Get participant info for a specified user/server combination
        /// </summary>
        /// <param name="user">DiscordUser being searched for</param>
        /// <param name="server">DiscordGuild (server) tied to record</param>
        /// <returns></returns>
        public Participant GetParticipantInfo(DiscordUser user, DiscordGuild server) =>
            collection.Find(x => x.User.Id == user.Id && x.Server.Id == server.Id).FirstOrDefault();

        /// <summary>
        /// Updates the participant info, inserting if missing.
        /// <para/>
        /// Returns updated participant value
        /// </summary>
        public Participant UpsertParticipant(Participant participant)
        {
            if(string.IsNullOrWhiteSpace(participant.Id))
            {
                participant.Id = ObjectId.GenerateNewId().ToString();
            }

            collection.ReplaceOne(x => x.User.Id == participant.User.Id && x.Server.Id == participant.Server.Id, participant, new ReplaceOptions() { IsUpsert = true });

            return GetParticipantInfo(participant);
        }

        /// <summary>
        /// Helper method fetches participant by object
        /// </summary>
        private Participant GetParticipantInfo(Participant participant) =>
            collection.Find(x => x.User.Id == participant.User.Id && x.Server.Id == participant.Server.Id).FirstOrDefault();
    }
}
