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
    /// <inheritdoc/>
    public class MongoDataAccess : IMongoDataAccess
    {
        private readonly string ParticipantCollectionName = "scores";
        private readonly string ArchiveCollectionName = "archive";
        private readonly string ConnectionString = "mongodb://192.168.1.69:27018";
        private readonly string DatabaseName = "shame_golf";

        private readonly IMongoCollection<Participant> particpantCollection;
        private readonly IMongoCollection<Archive> archiveCollection;

        /// <summary>
        /// IoC constructor
        /// </summary>
        public MongoDataAccess()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);

            particpantCollection = database.GetCollection<Participant>(ParticipantCollectionName);
            particpantCollection.Indexes.CreateOne(new CreateIndexModel<Participant>(Builders<Participant>.IndexKeys.Ascending(x => x.UserId)));

            archiveCollection = database.GetCollection<Archive>(ArchiveCollectionName);
            archiveCollection.Indexes.CreateOne(new CreateIndexModel<Archive>(Builders<Archive>.IndexKeys.Ascending(x => x.Id)));
        }

        public List<Participant> GetParticipants(ulong serverId) => particpantCollection.Find(x => x.ServerId == serverId).ToList();

        public Participant GetParticipantInfo(ulong userId, ulong serverId) =>
            particpantCollection.Find(x => x.UserId == userId && x.ServerId == serverId).FirstOrDefault();

        public (Participant, ReplaceOneResult) UpsertParticipant(Participant participant)
        {
            if(string.IsNullOrWhiteSpace(participant.Id))
            {
                participant.Id = ObjectId.GenerateNewId().ToString();
            }

            var result = particpantCollection.ReplaceOne(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId, participant, new ReplaceOptions() { IsUpsert = true });

            return (GetParticipantInfo(participant), result);
        }

        public List<string> GetArchivalNames(ulong serverId) => archiveCollection.Find(x => x.ServerId == serverId).ToList().Select(a => a.ArchiveName).Distinct().ToList();

        public Archive GetArchive(DiscordGuild server, string archiveName) => archiveCollection.Find(x => x.ServerId == server.Id && x.ArchiveName.Equals(archiveName)).FirstOrDefault();

        public void ArchiveSeason(ulong serverId, string archiveName)
        {
            var archive = new Archive()
            {
                ArchiveName = archiveName,
                Id = ObjectId.GenerateNewId().ToString(),
                ServerId = serverId,
                Participant = GetParticipants(serverId)
            };

            Console.WriteLine($"Archiving season. {JsonConvert.SerializeObject(archive)}");

            archiveCollection.InsertOne(archive);

            particpantCollection.DeleteMany(x => x.ServerId == serverId);
        }

        /// <summary>
        /// Helper method fetches participant by object
        /// </summary>
        private Participant GetParticipantInfo(Participant participant) =>
            particpantCollection.Find(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId).FirstOrDefault();
    }
}
