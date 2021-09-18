using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PuttPutt.Models;

namespace PuttPutt.DataAccess
{
    /// <summary>
    /// Provides access to underlying data store in Mongo
    /// </summary>
    public class MongoDataAccess
    {
        private readonly string ParticipantCollectionName = "scores";
        private readonly string ArchiveCollectionName = "archive";
        private readonly string ConnectionString = "mongodb://192.168.1.69:27018";
        private readonly string DatabaseName = "shame_golf";

        private readonly IMongoCollection<Participant> particpantCollection;
        private readonly IMongoCollection<Archive> archiveCollection;

        public MongoDataAccess()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);

            particpantCollection = database.GetCollection<Participant>(ParticipantCollectionName);
            particpantCollection.Indexes.CreateOne(new CreateIndexModel<Participant>(Builders<Participant>.IndexKeys.Ascending(x => x.UserId)));

            archiveCollection = database.GetCollection<Archive>(ArchiveCollectionName);
            archiveCollection.Indexes.CreateOne(new CreateIndexModel<Archive>(Builders<Archive>.IndexKeys.Ascending(x => x.Id)));
        }

        /// <summary>
        /// Returns an unsorted list of all particpant entries for a specified server
        /// </summary>
        public List<Participant> GetParticipants(DiscordGuild server) => particpantCollection.Find(x => x.ServerId == server.Id).ToList();

        /// <summary>
        /// Get participant info for a specified user/server combination
        /// </summary>
        /// <param name="user">DiscordUser being searched for</param>
        /// <param name="server">DiscordGuild (server) tied to record</param>
        /// <returns></returns>
        public Participant GetParticipantInfo(DiscordUser user, DiscordGuild server) =>
            particpantCollection.Find(x => x.UserId == user.Id && x.ServerId == server.Id).FirstOrDefault();

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

            particpantCollection.ReplaceOne(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId, participant, new ReplaceOptions() { IsUpsert = true });

            return GetParticipantInfo(participant);
        }

        /// <summary>
        /// Helper method fetches participant by object
        /// </summary>
        private Participant GetParticipantInfo(Participant participant) =>
            particpantCollection.Find(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId).FirstOrDefault();

        /// <summary>
        /// Gets all archive names from the specified server
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public List<string> GetArchivalNames(DiscordGuild server) => archiveCollection.Find(x => x.ServerId == server.Id).ToList().Select(a => a.ArchiveName).Distinct().ToList();

        /// <summary>
        /// Gets the archival entry for the specified server and name
        /// </summary>
        /// <param name="server"></param>
        /// <param name="archiveName"></param>
        /// <returns></returns>
        public Archive GetArchive(DiscordGuild server, string archiveName) => archiveCollection.Find(x => x.ServerId == server.Id && x.ArchiveName.Equals(archiveName)).FirstOrDefault();

        /// <summary>
        /// Moves records from current 
        /// </summary>
        /// <param name="server">Discord server for season</param>
        /// <param name="archiveName">Archival name given to season</param>
        public void ArchiveSeason(DiscordGuild server, string archiveName)
        {
            var archive = new Archive()
            {
                ArchiveName = archiveName,
                Id = ObjectId.GenerateNewId().ToString(),
                ServerId = server.Id,
                Participant = GetParticipants(server)
            };

            System.Console.WriteLine($"Archiving season. {JsonConvert.SerializeObject(archive)}");

            archiveCollection.InsertOne(archive);

            particpantCollection.DeleteMany(x => x.ServerId == server.Id);
        }
    }
}
